// Sadly #r directive for Playwright cannot work outright.
// Microsoft.Playwright package does not download the relevant browser binary along with it.
// So we need to go the full-fledged way to install playwright.
#r "nuget: Microsoft.Playwright"
#r "nuget: DotNetEnv, 2.5.0"

open Microsoft.Playwright
open System
open System.Threading.Tasks

DotNetEnv.Env.Load()

let url = "https://climateapp.net.au/"

let MakeBrowserContext () =
    task {
        let! pw = Playwright.CreateAsync()
        let opts = BrowserTypeLaunchOptions(Headless = false)
        let! browser = pw.Chromium.LaunchAsync(opts)
        return! browser.NewContextAsync()
    }

let MakeTab url (browserContextT: Task<IBrowserContext>) =
    task {
        let! context = browserContextT
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync(url)
        return page
    }

let SetAuthCookie (browserContextT: Task<IBrowserContext>) =
    task {
        let! context = browserContextT

        let cookie =
            Cookie(
                Name = ".AspNet.ApplicationCookie",
                Value = Environment.GetEnvironmentVariable("AUTH_COOKIE_VALUE"),
                Domain = ".climateapp.net.au",
                Path = "/",
                Expires = 1701401407f,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteAttribute.None
            )

        let! () = context.AddCookiesAsync([ cookie ])
        return context
    }

let getUser (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let ele = tab.GetByTitle("Manage")
        return! ele.TextContentAsync()
    }

let LoadHowsTheSeasonPage (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let! _ = tab.GotoAsync("https://climateapp.net.au/A03_HowsTheSeason")
        let waitingLocator = tab.GetByText("Preparing analysis")
        // let! () = Assertions.Expect(waitingLocator).Not.ToBeVisibleAsync()
        let! _ = tab.WaitForSelectorAsync(".analysis-content-ready")
        let! () = tab.WaitForTimeoutAsync(5000f)
        return tab
    }

let ScreenshotPage (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let opts = PageScreenshotOptions(FullPage = true, Path = "screenshot.jpg")
        let! _ = tab.ScreenshotAsync(opts)
        return tab
    }

let SavePage (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let! content = tab.ContentAsync()
        IO.File.WriteAllText("page.html", content)
        return tab
    }

let CloseBrowser (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let browser = tab.Context.Browser
        return! browser.CloseAsync()
    }

MakeBrowserContext ()
|> SetAuthCookie
|> MakeTab url
|> LoadHowsTheSeasonPage
|> ScreenshotPage
|> SavePage
|> CloseBrowser
|> Async.AwaitTask
|> Async.RunSynchronously
|> Console.WriteLine
