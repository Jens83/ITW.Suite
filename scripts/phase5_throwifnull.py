"""
Phase 5: Konstruktor-Boilerplate eliminieren
Ersetzt `_field = param ?? throw new ArgumentNullException(nameof(param));`
durch `ArgumentNullException.ThrowIfNull(param); _field = param;`
"""
import re
import sys
import glob

# Multiline pattern: _field = param\n    ?? throw new ArgumentNullException(nameof(param));
PATTERN_MULTI = re.compile(
    r'^([ \t]+)(_\w+) = (\w+)\r?\n[ \t]+\?\? throw new ArgumentNullException\(nameof\(\3\)\);',
    re.MULTILINE
)

# Single-line pattern: _field = param ?? throw new ArgumentNullException(nameof(param));
PATTERN_SINGLE = re.compile(
    r'^([ \t]+)(_\w+) = (\w+) \?\? throw new ArgumentNullException\(nameof\(\3\)\);',
    re.MULTILINE
)


def replace_multi(m):
    indent, field, param = m.group(1), m.group(2), m.group(3)
    return f'{indent}ArgumentNullException.ThrowIfNull({param});\n{indent}{field} = {param};'


def replace_single(m):
    indent, field, param = m.group(1), m.group(2), m.group(3)
    return f'{indent}ArgumentNullException.ThrowIfNull({param});\n{indent}{field} = {param};'


def transform(filepath):
    with open(filepath, encoding='utf-8-sig', newline='') as f:
        original = f.read()

    if 'throw new ArgumentNullException(nameof' not in original:
        return False

    content = PATTERN_MULTI.sub(replace_multi, original)
    content = PATTERN_SINGLE.sub(replace_single, content)

    if content == original:
        return False

    # Preserve original line-ending style
    if '\r\n' in original:
        content = content.replace('\n', '\r\n').replace('\r\r\n', '\r\n')

    with open(filepath, 'w', encoding='utf-8', newline='') as f:
        f.write(content)
    return True


def main():
    files = glob.glob('src/**/*.cs', recursive=True)
    changed, skipped = [], []

    for filepath in sorted(files):
        try:
            if transform(filepath):
                changed.append(filepath)
        except Exception as e:
            skipped.append((filepath, str(e)))

    print(f'Modified {len(changed)} files:')
    for f in changed:
        print(f'  {f}')

    if skipped:
        print(f'\nSkipped {len(skipped)} files (errors):')
        for f, err in skipped:
            print(f'  {f}: {err}')

    # Remaining count
    remaining = 0
    for filepath in files:
        try:
            with open(filepath, encoding='utf-8-sig') as f:
                content = f.read()
            remaining += content.count('throw new ArgumentNullException(nameof')
        except Exception:
            pass
    print(f'\nVerbleibende "throw new ArgumentNullException(nameof" Treffer: {remaining}')


if __name__ == '__main__':
    main()
