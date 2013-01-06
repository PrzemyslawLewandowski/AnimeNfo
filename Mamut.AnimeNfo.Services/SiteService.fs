﻿module Mamut.AnimeNfo.Services.SiteService

open Mamut.AnimeNfo.Services.SitePraser
open System.Net
open System.IO
open System.Linq


let asyncGetHtml (url : string) = async{
    let yearsRequest = WebRequest.Create(url)
    let! rsp = yearsRequest.AsyncGetResponse()
                
    use stream = rsp.GetResponseStream()
    use reader = new StreamReader(stream)
 
    return! Async.AwaitTask(reader.ReadToEndAsync())
    }

let animeByYearPage = asyncGetHtml "http://www.animenfo.com/animebyyear.html"

let yearUrls = async { 
    let! page = animeByYearPage
    return yearUrlsFromPage page
    }

let rec animeByYearUrls yearUrl = async {
    let! page = asyncGetHtml yearUrl 
    let urls = urlsFromPage page
    let nextUrlQuery = nextUrl page

    return 
        match nextUrlQuery with
        | None  -> urls
        | Some uq ->
            async{
            let! a = animeByYearUrls uq
            return Seq.append urls a}
            |> Async.RunSynchronously
    }