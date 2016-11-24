# Suave.Minify
A minify WebPart for Suave

In the following example the combinator `jsbundle "path"` compress all javascript files in the folder "js" and concatenates them in a single response.

```fsharp
open Suave
open Suave.Minify

let part : WebPart = Filters.path "/jsbundle" >=> jsBundle "js"
```

*Depends on YUICompressor.NET
