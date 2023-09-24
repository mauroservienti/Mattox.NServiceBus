---
name: Package release
about: Package release template
title: Release [version]
labels: ''
assignees: ''

---

- Prepare the release
  - [ ] tag the repo (packages will be pushed to the [Feedz.io](https://feedz.io) pre-releases feed)
  - [ ] update dependabot configuration to consider any newly created `release-X.Y` branch 
- Release notes:
  - [ ] edit release notes as necessary, e.g. to mention contributors
  - [ ] associate draft with the created tag
  - [ ] publish release notes
- Release
  - [ ] from [Feedz.io](https://feedz.io) push to Nuget
- Clean-up
- [ ] close this issue
- [ ] close the milestone
