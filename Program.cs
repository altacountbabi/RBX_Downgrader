using System;
using System.IO;
using System.Threading;
using Spectre.Console;

namespace RBX_Downgrader
{
    public class Program
    {
        static PSRunner ps = new PSRunner();
        static Downloader dl = new Downloader();

        public static void Main(string[] args)
        {
            try
            {
                string[] bundles = new string[1000]; // haha cant wait for a memory leak (or not)
                string[] allowed_bundles = {
                    "582",
                    "581",
                    "578"
                };

                bundles[582] = "https://download1337.mediafire.com/m9jdyrdsa2eg53e28px5YsCJ-ddHLphYH88uaop5W9gL7iF7o1Umrvk_aDPUg8-8KinFPQt_ZrZjD-d7FFXUcckCSI01-eZ4qJZxgHTB79oAu-GaOTxMtSo1uiQNS4drvYVtVJV25kkOwVQu5hg9BiaUj98A-BL3c7UnqjPPIq_x/mprz1vvx39h0r67/ROBLOXCORPORATION.ROBLOX_2.582.384.0_neutral_~_55nm5eh3cm0pr.Msixbundle";
                bundles[581] = "https://download2441.mediafire.com/vkg01d4s6gdg6HgZSh85rfvIsmg-NK7Mb08Uc9SkFrZrY0Gw1hsqLIQpvxiLd1VSoUsZYsn604e7ll1LuBedE5uucvMTiExceGjarLnuotqbjSI3R_CjP6ra9s1Xr8OXGbm7Er628bGZKXp2tkUzcL_KTLzKFDfIyaUwBZqLbhvJ/3fisz87r45ao851/ROBLOXCORPORATION.ROBLOX_2.581.563.0_neutral_~_55nm5eh3cm0pr.Msixbundle";
                bundles[578] = "https://download2293.mediafire.com/hfmectuucvvgNtQxoVFvaJ0M5SQWo0Gusa97QH5QCevmCTtKEN90k-amRrEOswhhNAqd1XdcU5U11kGreliSaFuHiyuG7q03w8TXbzNAQ97-05S_WxdjVbxgJjkHBT2iRUB46RKLL-Dc_HjGUAQQTOvbpMXb2BBWMsc9kj8fTarJ/xaxisa2v54rk8ja/ROBLOXCORPORATION.ROBLOX_2.578.564.0_neutral_~_55nm5eh3cm0pr.Msixbundle";

                Console.Title = "RBX Downgrader";

                var y_n_uninstall_warning = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Warning![/] This program will uninstall your current version of roblox, this is a required step for downgrading roblox, do you wish to continue?\n\n[grey]use arrow keys![/]")
                        .PageSize(10)
                        .AddChoices(new[] { "Yes", "No" }));

                if (y_n_uninstall_warning == "No") Environment.Exit(0);

                ps.execute("Get-AppxPackage | Where-Object { $_.Name -like '*blox*' } | Remove-AppxPackage", (string data) =>
                {
                    if (data != "")
                    {
                        throw new Exception($"Possible error: {data}");
                    }
                });

                AnsiConsole.Clear();

                AnsiConsole.Markup("[green]Successfully Uninstalled roblox[/]\n\n");

                var chosen_bundle = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What roblox verison would you like to downgrade to?\n\n[grey]use arrow keys![/]")
                        .PageSize(10)
                        .AddChoices(allowed_bundles));

                if (!Directory.Exists("bundles")) Directory.CreateDirectory("bundles");


                string bundle_url = bundles[int.Parse(chosen_bundle)];
                string bundle_name = "";

                int lastIndex = bundle_url.LastIndexOf("/");
                if (lastIndex >= 0 && lastIndex < bundle_url.Length - 1) bundle_name = bundle_url.Substring(lastIndex + 1);

                dl.download($"version {chosen_bundle} (this may take a while)", bundle_url, $"bundles/{bundle_name}");

                AnsiConsole.Clear();
                AnsiConsole.Markup($"[green]Successfully downloaded version {chosen_bundle}[/]\n\n");

                AnsiConsole.Status()
                    .Start($"Installing version {chosen_bundle}.. (this may take a while)", ctx =>
                    {
                        ps.execute($"Add-AppxPackage -Path bundles/{bundle_name}");
                        Thread.Sleep(1500);
                    });


                AnsiConsole.Clear();
                AnsiConsole.Markup($"[green]Successfully downgraded to version {chosen_bundle}[/]\n[underline]Have fun playing![/]");
                File.Delete($"bundles/{bundle_name}");
                Console.ReadLine();
            } catch (Exception ex)
            {
                AnsiConsole.Clear();
                AnsiConsole.Markup("[underline]REPORT THIS TO @whoman0385 ON DISCORD[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
                Console.ReadLine();
            }
        }
    }
}
