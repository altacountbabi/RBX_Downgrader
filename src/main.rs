use std::process::{Command, Stdio};
use reqwest::Client;
use anyhow::{Result, Ok};
use std::fs::{File, remove_file};
use std::io::copy;
use indicatif::ProgressBar;
use std::io::{self, Write};

fn clear_console() {
    let mut stdout = std::io::stdout();
    write!(stdout, "\x1B[2J\x1B[1;1H").unwrap();
}

async fn download_file(url: String, save_path: &str, chunk_size: usize) -> Result<()> {
    let bar = ProgressBar::new(146);
    let client = Client::new();
    let file = File::create(save_path)?;
    let mut start_byte = 0;

    let mut tasks = Vec::new();

    let client = client.clone();
    let file_clone = file.try_clone()?;
    let url_clone = url.clone();

    let task = tokio::task::spawn(async move {
        let mut file = file_clone;
        let url = url_clone;

        loop {
            let end_byte = start_byte + chunk_size - 1;
            let response = client
                .get(&url)
                .header("Range", format!("bytes={}-{}", start_byte, end_byte))
                .send()
                .await?;

            if response.status().is_success() {
                let bytes = response.bytes().await?;
                copy(&mut bytes.as_ref(), &mut file)?;
            } else {
                bar.finish();
                break; // Reached the end of the file
            }

            start_byte += chunk_size;
            bar.inc(1);
        }
        Result::<_, anyhow::Error>::Ok(())
    });

    tasks.push(task);

    for task in tasks {
        task.await??;
    }

    Ok(())
}

fn run_ps<F: FnMut(String) -> ()>(code: &str, mut callback: F) {
    let output = Command::new("powershell")
        .args(&[
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-Command",
            code
        ])
        .stdout(Stdio::piped())
        .output()
        .expect("Failed to run PowerShell command.");

    let output_string = String::from_utf8_lossy(&output.stdout);
    callback(output_string.to_string());
}

fn get_roblox_ver() -> u32 {
    let mut version: String = String::from("");

    let output = Command::new("powershell")
        .args(&[
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-Command",
            "(Get-AppxPackage | Where-Object { $_.Name -like '*ROBLOXCORPORATION.ROBLOX*' }).Version"
        ])
        .stdout(Stdio::piped())
        .output()
        .expect("Failed to run PowerShell command.");


    let output_string = String::from_utf8_lossy(&output.stdout);

    if output_string != "" {
        version = output_string.to_string()
    }

    let parts: Vec<&str> = version.split('.').collect();
    let number: u32 = parts[1].parse().unwrap_or(0);

    return number
}

fn install_bundle(bundle_path: &str) {
    run_ps(format!("Add-AppxPackage -Path {}", bundle_path).as_str(), |output| { let _ = output; });
}

#[tokio::main]
async fn main() -> Result<()> {
    let mut stop: bool = false;
    let bundle581 = "https://files.catbox.moe/1kd5gv.Msixbundle";
    let mut roblox_installed: bool = true;

    run_ps("Get-AppxPackage | Where-Object { $_.Name -like '*ROBLOXCORPORATION.ROBLOX*' }", |output| {
        if output == "" {
            roblox_installed = false;
        }
    });

    if roblox_installed {
        let roblox_version = get_roblox_ver();
        if roblox_version == 581 {
            println!("Already on working version");
            stop = true;
        }
    }

    if !stop {
        let mut stdout = std::io::stdout();
        write!(stdout, "\x1B[?25l").unwrap();
        println!("Preparing to downgrade roblox...");
        clear_console();
        run_ps("Get-AppxPackage | Where-Object { $_.Name -like '*ROBLOXCORPORATION.ROBLOX*' } | Remove-AppxPackage", |output| {let _ = output; });
        clear_console();
        println!("Downloading required files...\n");
        download_file(bundle581.into(), "rbx.Msixbundle", 1024*1024).await?;
        clear_console();
        println!("Downgrading...");
        install_bundle("rbx.Msixbundle");
        clear_console();
        let _ = remove_file("rbx.Msixbundle");
        println!("Successfully downgraded roblox.\n\nIf you see a button saying \"Retry\" when opening roblox, simply spam it until its gone.");
        write!(stdout, "\x1B[?25h").unwrap();
    }

    let mut input = String::new();
    io::stdin().read_line(&mut input).unwrap();

    Ok(())
}
