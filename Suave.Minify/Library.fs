module Suave.Minify

open System
open System.IO
open System.Text

open Suave
open Suave.Operators

open Yahoo.Yui.Compressor

let private jsCompressor  = JavaScriptCompressor()
let private cssCompressor = CssCompressor()

let mimify files filter =
  let output = new StringBuilder()

  let compressor =
    if filter = "*.js" then 
      jsCompressor  :> Compressor
    elif filter = "*.css" then 
      cssCompressor :> Compressor
    else
      failwithf "Unsupported filter %s, only *.js and *.css are supported." filter
  
  for file in files do
    try
      let txt = File.ReadAllText(file)
      if file.Contains(".min.") then
        output.Append (txt)  |> ignore
      else
        let compressed = compressor.Compress(txt)
        output.Append (compressed)  |> ignore
    with
      | :? EcmaScript.NET.EcmaScriptException as exp ->
        failwithf "error minifying %s, line no: %d, error: %s" file exp.LineNumber exp.Message
      | exp ->
        failwithf "error minifying %s : %s" file exp.Message
  
  output.ToString()

let bundlePart files filter : WebPart =
  context(fun ctx ->
    match ctx.request.header "if-modified-since" with
    | Choice1Of2 v ->
      match Utils.Parse.dateTime v with
      | Choice1Of2 date ->
        let lastModified = Seq.max (Seq.map (fun (x:FileInfo) -> x.LastWriteTimeUtc) (Seq.map (fun x -> new FileInfo(x)) files))
        if lastModified > date then
          let output = mimify files filter
          Successful.OK output
        else
          Redirection.NOT_MODIFIED
      | Choice2Of2 parse_error -> RequestErrors.bad_request [||]
    | Choice2Of2 _ ->
      let output = mimify files filter
      Successful.OK output)

let bundle files filter =
  context(fun ctx ->
    bundlePart (Seq.map (fun x -> Files.resolvePath ctx.runtime.homeDirectory x) files) filter
  )

let jsBundle path : WebPart =
  Writers.setMimeType "application/x-javascript" >=> bundle path "*.js"

let cssBundle path : WebPart =
  Writers.setMimeType "text/css" >=> bundle path "*.css"