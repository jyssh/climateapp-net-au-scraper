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

module AnalysisPage =
    type T =
        | HowsTheSeason
        
    let ToUrl t =
        match t with
        | HowsTheSeason -> "https://climateapp.net.au/A03_HowsTheSeason"
        
    let ToStem t =
        // stem is filename without extension
        // https://stackoverflow.com/a/72803594
        match t with
        | HowsTheSeason -> "hows-the-season"
    
    let Load name (tabTask: Task<IPage>) =
        let url = ToUrl name
        task {
            let! tab = tabTask
            let! _ = tab.GotoAsync(url)
            
            let waitingLocator = tab.GetByText("Preparing analysis")
            let waitingOpts = LocatorWaitForOptions(State=WaitForSelectorState.Hidden)
            let! () = waitingLocator.WaitForAsync(waitingOpts)
            
            let readyLocator = tab.Locator(".analysis-content-ready")
            let readyOpts = LocatorWaitForOptions(State=WaitForSelectorState.Visible)
            let! () = readyLocator.WaitForAsync(readyOpts)
            
            return tab
        }

let SaveAsImage path (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let opts = PageScreenshotOptions(FullPage = true, Path = path, Animations=ScreenshotAnimations.Disabled)
        let! _ = tab.ScreenshotAsync(opts)
        return tab
    }

let SaveAsHtml path (tabTask: Task<IPage>) =
    task {
        let! tab = tabTask
        let! content = tab.ContentAsync()
        IO.File.WriteAllText(path, content)
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
|> AnalysisPage.Load AnalysisPage.HowsTheSeason
|> SaveAsImage $"{AnalysisPage.ToStem AnalysisPage.HowsTheSeason}.png"
|> SaveAsHtml $"{AnalysisPage.ToStem AnalysisPage.HowsTheSeason}.html"
|> CloseBrowser
|> Async.AwaitTask
|> Async.RunSynchronously
