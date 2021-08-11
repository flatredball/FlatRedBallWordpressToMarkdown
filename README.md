# FlatRedBallWordpressToMarkdown

This is a very hacky start to converting FlatRedBall docs, which are currently hosted in WordPress, to markdown files that are easy to transport and host on SaaS solutions.

The full project spec is here:
https://docs.google.com/document/d/1Jz7oywrmTa9tEaEnuoFd2XZtKspCwkVmGFNWiYwdSLI/edit?usp=sharing

## Notes

1. This program uses Pandoc to convert HTML to Markdown and expects that to be in your path
2. This uses the API instead of a MySQL dump because I'm not sure the server is powerful enough to execute a dump
3. The API scrape must be throttled so as not to bring the server down
4. IIRC the current iteration only fetches published pages, not posts
