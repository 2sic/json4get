# JSON 4 GET - HTTP-Get-Request Adapter

## What we're fixing

Passing JSON in a GET request is ugly - really, really ugly. Even simple objects become completely unreadable, like:

```javascript
// what we want
/api?p={"filters": {"completed": true, "tags": ["issues", "best practices"]} }

// what we get
/api?p=%7B%22filters%22%3A%20%7B%22completed%22%3A%20true%2C%20%22tags%22%3A%20%5B%22issues%22%2C%20%22best%20practices%22%5D%7D%20%7D

// this shorter, more readable version would be much nicer
/your-path-here?params=('filters':('completed':true,'tags':!"issues","best_practices"!))
```

So the ugly result is 20% - 200% larger, unreadable and not editable by a human, because using JSON in a url ([specs](https://www.ietf.org/rfc/rfc1738.txt)) and URI ([specs](https://www.ietf.org/rfc/rfc3986.txt)) was never a design goal. For example, the most used characters in a JSON are `{`, `}`, `"`, `[`, `]` and space - all of which are mapped to 3 characters like `%7B`, `%7D` etc. And if the JSON contains valid white-space between objects, it becomes even uglier.

## How we're fixing it

We define a standard for substituting common JSON characters with safe URL characters, while preserving the information. This is solved at two levels: _1) in the structure_ and _2) inside value nodes_.

### Substitutions in the Structure, _Outside_ of Values

* all whitespace is removed as it has no relevance outside of values
* `{` and `}` become `(` and `)`
* `"` becomes `'`
* `[` and `]` both become `!`

This is simple and straight-forward, and all the substituted characters can never occur in a JSON outside of a value node. 

### Substitutions _Inside_ Values

* `'` becomes `\'` to differentiate value delimiters (used in the structure) from simple apostrophes
* space becomes `_` as spaces are very common in strings, while `_` are very rare
* `_` becomes `\_` because now the basic `_` is used for spaces

## Implemented Programming Languages

As of 2019-01 we are implementing in in .net 2.0, .net core and JavaScript. 