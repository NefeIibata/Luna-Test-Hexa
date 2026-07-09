import re

html = open(r'C:\Users\Nefelibata\.gemini\antigravity\brain\ba977bd1-2a3c-49e8-a882-57eba79e542c\.system_generated\steps\4\content.md', 'r', encoding='utf-8').read()

matches = re.findall(r'"([^"]*[А-Яа-яЁё][^"]*)"', html)
text = re.sub(r'<[^>]+>', '\n', html)
text = re.sub(r'\n\s*\n', '\n', text)

with open('task_text.txt', 'w', encoding='utf-8') as f:
    f.write('=== JS String matches ===\n')
    seen = set()
    for m in matches:
        if m not in seen:
            f.write(m + '\n')
            seen.add(m)
    f.write('\n=== Tag Stripped Text ===\n')
    for line in text.split('\n'):
        if re.search(r'[А-Яа-яЁё]', line):
            f.write(line.strip() + '\n')
