# Typesets Part III (chapters 21-24) from manuscripts/ into part3.html,
# reusing the demo page's exact CSS so the two artifacts read as one book.
import re
from pathlib import Path

import markdown

HERE = Path(__file__).parent
MD_EXT = ["fenced_code", "tables", "codehilite"]
MD_CFG = {"codehilite": {"guess_lang": False, "linenums": False}}

CHAPTERS = ["ch21", "ch22", "ch23", "ch24"]


def convert(md_text: str) -> str:
    md_text = md_text.replace("- [ ] ", '- <span class="cb"></span> ')
    html = markdown.markdown(md_text, extensions=MD_EXT, extension_configs=MD_CFG)
    html = html.replace("<table>", '<div class="tablewrap"><table>').replace(
        "</table>", "</table></div>"
    )
    html = re.sub(
        r"<blockquote>\s*<p><strong>Practice rules",
        '<blockquote class="contract">\n<p><strong>Practice rules',
        html,
    )
    return html


# CSS: lift the style block from the demo page verbatim.
demo_html = (HERE / "book-demo" / "book-demo.html").read_text(encoding="utf-8")
css = re.search(r"<style>(.*?)</style>", demo_html, re.S).group(1)

sections = []
nav_links = []
for key in CHAPTERS:
    md_text = (HERE / "manuscripts" / f"{key}.md").read_text(encoding="utf-8")
    lines = md_text.split("\n")
    h1 = lines[0].lstrip("# ").strip()
    ch_num, ch_title = [s.strip() for s in h1.split("—", 1)]
    body = convert("\n".join(lines[1:]).strip())
    nav_links.append(f'<a href="#{key}">{ch_num.replace("Chapter ", "Ch. ")}</a>')
    sections.append(f"""
<section class="doc" id="{key}">
  <div class="opener">
    <div class="kicker">The BookIt Arc &middot; Part III</div>
    <div class="chnum">{ch_num}</div>
    <h1>{ch_title}</h1>
  </div>
  <div class="chapter-body">
  {body}
  </div>
</section>""")

overview_md = (HERE / "part3-overview.md").read_text(encoding="utf-8")
overview = convert(overview_md)

page = f"""<title>The Blank-Page Reflex — Part III: The BookIt Arc</title>
<style>{css}</style>

<nav class="site-nav">
  <span class="slug">The Blank-Page Reflex · Part III</span>
  <span class="links">
    <a href="#overview">The Arc</a>
    {' '.join(nav_links)}
  </span>
</nav>

<header class="cover">
  <div class="eyebrow">Part III · Design Patterns in Production</div>
  <h1>The BookIt Arc</h1>
  <p class="subtitle">Chapters 21–24 — one system, four patterns, every test executed</p>
  <div class="rule"></div>
  <div class="slip">
    Proof section &middot; continues Chapter 20 (see the Book Demo artifact)<br>
    Every milestone committed &middot; every red run captured &middot; tags <strong>bookit-ch21 … bookit-ch24</strong>
  </div>
</header>

<section class="doc" id="overview">
  <div class="opener">
    <div class="kicker">00 &middot; The Arc So Far</div>
    <h1>One System, Five Chapters</h1>
  </div>
  {overview}
</section>
{''.join(sections)}

<footer class="colophon">
  Part III proof section &middot; typeset 09/07/2026 &middot; set in Palatino &amp; Cascadia Code<br>
  Every listing extracted from the executed BookIt repository &middot; companion history: M1&ndash;M5 + ch21&ndash;ch24
</footer>
"""

(HERE / "part3.html").write_text(page, encoding="utf-8")
print(f"part3.html written: {len(page):,} chars")
