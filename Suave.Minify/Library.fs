module Suave.Minify

open System
open System.IO
open System.Text

open Suave
open Suave.Operators

open Yahoo.Yui.Compressor

let private jsCompressor  = JavaScriptCompressor()
let private cssCompressor = CssCompressor()

let mimify path filter =
  let output = new StringBuilder()

  let compressor =
    if filter = "*.js" then 
      jsCompressor  :> Compressor
    elif filter = "*.css" then 
      cssCompressor :> Compressor
    else
      failwithf "Unsupported filter %s, only *.js and *.css are supported." filter
  
  for file in Directory.EnumerateFiles(path,filter) do
    try
      let txt = File.ReadAllText(file)
      let compressed = compressor.Compress(txt)
      output.Append (compressed)  |> ignore
    with
      | :? EcmaScript.NET.EcmaScriptException as exp ->
        failwithf "error minifying %s, line no: %d, error: %s" file exp.LineNumber exp.Message
      | exp ->
        failwithf "error minifying %s : %s" file exp.Message
  
  let str = output.ToString()
  Console.WriteLine("bundle.Length = {0}",str) 
  str


let bundle path filter : WebPart =
  context(fun ctx ->
    let realpath = Files.resolvePath ctx.runtime.homeDirectory path
    match ctx.request.header "if-modified-since" with
    | Choice1Of2 v ->
      match Utils.Parse.dateTime v with
      | Choice1Of2 date ->
        let files = Seq.map (fun x -> new FileInfo(x)) (Directory.EnumerateFiles(realpath,filter))
        let lastModified = Seq.max (Seq.map (fun (x:FileInfo) -> x.LastWriteTimeUtc) files)
        if lastModified > date then
          let output = mimify realpath filter
          Successful.OK output
        else
          Redirection.NOT_MODIFIED
      | Choice2Of2 parse_error -> RequestErrors.bad_request [||]
    | Choice2Of2 _ ->
      let output = mimify realpath filter
      Successful.OK output)


let jsBundle path : WebPart =
  Writers.setMimeType "application/x-javascript" >=> bundle path "*.js"

let cssBundle path : WebPart =
  Writers.setMimeType "text/css" >=> bundle path "*.css"