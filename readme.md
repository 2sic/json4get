# JSON 4 GET - HTTP-Get-Request Adapter

## What we're fixing

Passing JSON in a GET request is really ugly. Even simple objects become completely unreadable, like:

```javascript
// what we want
/api?p={"filters": {"completed": true, "tags": ["issues", "best practices"]} }

// what we get
/api?p=%7B%22filters%22%3A%20%7B%22completed%22%3A%20true%2C%20%22tags%22%3A%20%5B%22issues%22%2C%20%22best%20practices%22%5D%7D%20%7D

// this shorter, more readable version would be much nicer
/api?p=('filters'!('completed'!t*'tags'!L'issues'*'best_practices'J))

// or when using these params for SPA use cases
/view#p=('filters'!('completed'!t*'tags'!L'issues'*'best_practices'J))
```

So the ugly result is 20% - 200% larger, unreadable and not editable by a human, because using JSON in a url fragment ([specs](https://www.ietf.org/rfc/rfc1738.txt)) and URI ([specs](https://www.ietf.org/rfc/rfc3986.txt)) was never a design goal. For example, the most used characters in a JSON are `{`, `}`, `:`, `"`, `[`, `]` and space - all of which are mapped to 3 characters like `%7B`, `%7D` etc. And if the JSON contains valid white-space between objects, it becomes even uglier.

The only characters allowed in a URL-fragment are `a-z`, `A-Z`, `0-9`, and `-_.!*'()`, so **Json4Get** addresses this missmatch.

## How we're fixing it

The goal of **Json4Get** is to shorten and reformat JSON to use as little URL-encoding as possible, while making it easy to read and fairly easy to modify manually (to test other parameters). To do this, we define a standard for substituting common JSON characters and words with safe URL characters, while preserving the information. This is solved at two levels: _1) in the structure_ and _2) inside value nodes_.

### Substitutions in the Structure, _Outside_ of Values

JSON puts just about everything between `"` quotes to mark values, except for `{},.:-0123456789` and `null,true,false`. To enable JSON in a URL-Fragment, the characters we could use instead are `()!*_'` and any `a-Z` character to represent the structure. Let's do this:

* all whitespace is removed as it has no relevance outside of values
* the three possible constants true/false/null are shortened to t/f/n
* `{` and `}` become `(` and `)` for easy readibility (looks similar)
* `"` becomes `'` for easy readibility (looks similar)
* `:` becomes `!` for easy readibily (looks similar)
* `,` becomes `*` as it's a clear separation character (`-` is not an option, as it's used for values)
* `[` and `]` become `L` and `J` as it looks a bit similar

This is simple and straight-forward, and all the substituted characters can never occur in a JSON outside of a value node.

### Substitutions _Inside_ Values

* `'` becomes `\'` to differentiate value delimiters (used in the structure) from simple apostrophes
* space becomes `_` as spaces are very common in strings and would otherwise become `%20`, while `_` are very rare
* `_` becomes `\_` because the basic `_` is used for spaces

## Implemented Programming Languages

As of 2019-01 we are implementing in in .net 2.0, .net core and JavaScript.

Any contributions in other languages would be greatly appreciated. 