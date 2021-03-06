﻿module SystemUtilities
    open SystemTypeExtensions
    open System
    open System.IO
    open System.Net
    open HtmlAgilityPack
    open System.Threading

    let allCardinalNumbers = {1..10000}

    /// Are we running on linux?
    let isLinuxFileSystem =
        let os = Environment.OSVersion
        let platformId = os.Platform
        match platformId with
            | PlatformID.Win32NT | PlatformID.Win32S | PlatformID.Win32Windows | PlatformID.WinCE | PlatformID.Xbox -> false
            | PlatformID.MacOSX | PlatformID.Unix -> true
            | _ ->false
    /// OS-independent file copy from one place to another. Uses shell.
    let copyToDestinationDirectory (localFileName:string) (copyTo:string) =
        if System.IO.File.Exists(localFileName) = false
            then
                ()
            else
                if not isLinuxFileSystem
                    then
                        let systemProc = new System.Diagnostics.Process()
                        systemProc.EnableRaisingEvents<-false
                        systemProc.StartInfo.FileName<-"cmd.exe"
                        systemProc.StartInfo.Arguments<-("/C copy " + localFileName + " " + copyTo)
                        systemProc.Start() |> ignore
                        systemProc.WaitForExit()                
                    else
                        let systemProc = new System.Diagnostics.Process()
                        systemProc.EnableRaisingEvents<-false
                        systemProc.StartInfo.FileName<-"/bin/cp"
                        systemProc.StartInfo.Arguments<-(" " + localFileName + " " + copyTo)
                        //System.Console.WriteLine (systemProc.StartInfo.FileName + systemProc.StartInfo.Arguments)
                        systemProc.Start() |> ignore
                        systemProc.WaitForExit()

                
    let getOrMakeDirectory dirName =
        if System.IO.Directory.Exists dirName
            then System.IO.DirectoryInfo dirName
            else System.IO.Directory.CreateDirectory dirName

    let forceDirectoryCreation (fullDirectoryName:string) =
        if  Directory.Exists(fullDirectoryName)
            then fullDirectoryName
            else Directory.CreateDirectory(fullDirectoryName).FullName


    /// Eliminates duplicate items in a list. Items must be comparable
    let removeDuplicates a =
        a |> List.fold(fun acc x->
            let itemCount = (a |> List.filter(fun y->x=y)).Length
            if itemCount>1
                then
                    if acc |> List.exists(fun y->x=y)
                        then
                            acc
                        else
                            List.append acc [x]
                else
                    acc
            ) []

    let batchesOf n =
        Seq.mapi (fun i v -> i / n, v) >>
        Seq.groupBy fst >>
        Seq.map snd >>
        Seq.map (Seq.map snd)
    let batchesOfNBy f n =
        Seq.mapi (fun i v -> i / (f n), v) >>
        Seq.groupBy fst >>
        Seq.map snd >>
        Seq.map (Seq.map snd)

    let removeDuplicatesBy f a:'a[] =
        a |> List.fold(fun acc x->
            if acc.Length>0
                then
                    if acc |> Array.exists(fun y->(f y)=(f x))
                        then
                            acc
                        else
                            [|x|]|>Array.append acc 
                else
                    [|x|]
            ) [||]
    /// Finds only the duplicates in a list. Items must be comparable
    let duplicates a =
        a |> List.fold(fun acc x->
            let itemCount = (a |> List.filter(fun y->x=y)).Length
            if itemCount>1 then List.append acc [x] else acc
            ) []
    /// finds only the duplicates in a list by applying a function to to each item
    /// new items must be comparable
    let duplicatesBy f a =
        a |> List.fold(fun acc x->
            let itemCount = (a |> List.filter(fun y->f x y)).Length
            if itemCount>1 then List.append acc [x] else acc
            ) []
    let prependToDelimitedList (prependString:string) (currentString:string) (newStringItem:string) =
        let prepend = if currentString.Length=0 || (currentString.GetRight 1) = prependString
                        then ""
                        else prependString.ToString()
        if newStringItem.Length=0 then currentString else
            (currentString + prepend + newStringItem)

    /// Create a dummy file in the OS and return a .NET FileInfo object. Used as a mock for testing
    let getFakeFileInfo() =
        let rndPrefix = System.IO.Path.GetRandomFileName()
        let tempFileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, (System.AppDomain.CurrentDomain.FriendlyName + rndPrefix + "_tester.bsx"))
        let fs1=System.IO.File.OpenWrite(tempFileName)
        let sw1=new System.IO.StreamWriter(fs1)
        sw1.WriteLine("test")
        sw1.Close()
        fs1.Close()
        let ret=new System.IO.FileInfo(tempFileName)
        ret
    // memoize one to reuse
    let dummyFileInfo = getFakeFileInfo()

    let agentArray = [|
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; ServiceUI 9) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063";
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5 (.NET CLR 3.5.30729)";
        |]
    let referralArray = [|
            "https://www.google.com";
            "https://www.bing.com";
            "https://www.duckduckgo.com";
            "https://www.yahoo.com";
            ""
        |]
    let makeWebClient  = 
        let client = new System.Net.WebClient();
        let newUserAgentHeader = agentArray.randomItem
        let newReferrer = referralArray.randomItem
        client.Headers.Add(HttpRequestHeader.UserAgent, newUserAgentHeader)
        client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36")
        client.Headers.Add(HttpRequestHeader.Referer, "https://www.google.com")
        client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml")
        client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5")
        client.Headers.Add(HttpRequestHeader.Upgrade, "1")
        client.Headers.Add("DNT", "1")
        client
    /// Fetch the contents of a web page
    let rec http (url:string) (tryCount:int)  =
        try
            ServicePointManager.ServerCertificateValidationCallback<-(new Security.RemoteCertificateValidationCallback(fun a b c d->true))
            //ServicePointManager.SecurityProtocol<-(SecurityProtocolType.Ssl3 ||| SecurityProtocolType.Tls12 ||| SecurityProtocolType.Tls11 ||| SecurityProtocolType.Tls)
            ServicePointManager.SecurityProtocol<-(SecurityProtocolType.Tls12 ||| SecurityProtocolType.Tls11 ||| SecurityProtocolType.Tls)
            HtmlAgilityPack.HtmlWeb.PreRequestHandler(fun webRequest->
                webRequest.MaximumAutomaticRedirections<-5
                webRequest.MaximumResponseHeadersLength<-4
                webRequest.Timeout<-15000
                webRequest.Credentials<-CredentialCache.DefaultCredentials
                webRequest.IfModifiedSince<-DateTime.Now.AddDays(-1.0)
                true
                ) |>ignore
            let Client = makeWebClient
            let strm = Client.OpenRead(url)
            let sr = new System.IO.StreamReader(strm)
            let html = sr.ReadToEnd()
            html
        with
            | :? System.Net.WebException as webex ->
                let newTryCount=tryCount+1
                if newTryCount<3
                    then
                        printfn "Retry %d"  (newTryCount+1) |> ignore
                        http url newTryCount
                    else ""
            | :? System.IO.IOException as iox->
                printfn "iox Exception %s" iox.Message
                ""
            | ex ->
                System.Console.WriteLine("Exception in Utils.http trying to load " + url)
                System.Console.WriteLine(ex.Message)
                System.Console.WriteLine(ex.StackTrace)
                if ex.InnerException = null
                    then
                        ""
                    else
                        System.Console.WriteLine("Inner")
                        System.Console.WriteLine(ex.InnerException.Message)
                        ""
    let withTimeout (timeOut: option<int>) (operation: Async<'x>) : Async<option<'x>> =
      match timeOut with
       | None -> async {
           let! result = operation
           return Some result
         }
       | Some timeOut -> async {
           let! child = Async.StartChild (operation, timeOut)
           try
             let! result = child
             return Some result
           with :? System.TimeoutException ->
             return None
         }

    let webclientLoadDoc url =
        let htmlText=http url 3
        let doc = new HtmlAgilityPack.HtmlDocument()
        doc.LoadHtml htmlText
        doc
    let browserLoadDoc (url:string) =
        let tOut=new System.Timers.Timer((float)60000)
        try
            tOut.Elapsed.Add(fun x->
                tOut.Stop()
                tOut.Enabled<-false
                tOut.Dispose()
                ())
            tOut.Start()
            let web=new HtmlWeb()
            let doc:HtmlDocument=web.Load(url) //.LoadFromBrowser url
            doc.ToString()
        with
        | :? System.TimeoutException as tEx->
            printfn "TimeoutException trying to use Browser Load"
            tOut.Stop()
            tOut.Enabled<-false
            tOut.Dispose()
            ""
        | ex ->
            printfn "System Exception trying to use Browser Load - %s" ex.Message
            tOut.Stop()
            tOut.Enabled<-false
            tOut.Dispose()
            ""
    let downloadFile (url:System.Uri) (fileName:string) = 
        let Client = makeWebClient 
        Client.DownloadFile(url, fileName)
        ()

