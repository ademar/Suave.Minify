# Suave.Minify
A minify WebPart for Suave

In the following example the combinator `jsbundle file-list` compress all javascript files in the list and concatenates them into a single response.

```fsharp
open Suave
open Suave.Minify

let part : WebPart =
  Filters.path "/jsbundle" >=> jsBundle ["/js/jquery-3.1.1.min.js"; "js/chess.js"; "js/app.js"]
```

*Depends on YUICompressor.NET
