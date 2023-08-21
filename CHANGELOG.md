# Changelog

## [Unreleased]

## [3.0.5-beta] - 2023-08-21

### Fixed

* Options window should always enable OK and Cancel buttons (#74)
* Fixed issue with deleting scheduled task after it's been disabled in options (#97)

## [3.0.4-beta] - 2023-08-19

### Fixed

* Task Scheduler and Options fixes by @DavidMoore in https://github.com/DavidMoore/ipfilter/pull/95
  - Renamed "Save Settings" button in Options to "OK", and it automatically closes the window after saving (#74)
  - Scheduled task should be created or deleted according to the chosen option (#72) 
* The status / error message should auto-size to fit in the window (#67)
* Corrected the URL in Add/Remove Programs entry

## [3.0.3-beta] - 2023-08-18

### Changed
* Build & installer no longer signed with code signing certificate
* Updated to .NET Framework 4.8

### Fixed
* Fixed eMule alternative config locations #68 
* Fixed saving / loading of options including the scheduled task #89 
* Fix startup crash by @yfdyh000 in https://github.com/DavidMoore/ipfilter/pull/66

## Contributors
* @DavidMoore
* @yfdyh000 made their first contribution in https://github.com/DavidMoore/ipfilter/pull/66

## [3.0.2.9-beta]

### Changed

- Changing the new upgrade logic

## [3.0.2.7-beta]

## [3.0.2.4-beta]

### Fixed

- Fixing problem with empty paths when trying to detect installed torrent clients (#59)

## [3.0.2.3-beta] - 2019-08-30

### Fixed

- Silent updating always installs 0 byte ipfilter.dat file (#58)

## [3.0.1.4-beta]

[Unreleased]: https://github.com/DavidMoore/ipfilter/compare/3.0.5-beta...HEAD
[3.0.5-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.4-beta...3.0.5-beta
[3.0.4-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.3-beta...3.0.4-beta
[3.0.3-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.2.9-beta...3.0.3-beta
[3.0.2.9-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.2.7-beta...3.0.2.9-beta
[3.0.2.7-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.2.4-beta...3.0.2.7-beta
[3.0.2.4-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.2.3-beta...3.0.2.4-beta
[3.0.2.3-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.1.4-beta...3.0.2.3-beta
[3.0.1.4-beta]: https://github.com/DavidMoore/ipfilter/compare/3.0.0-beta1...3.0.1.4-beta
[3.0.0-beta1]: https://github.com/DavidMoore/ipfilter/releases/tag/3.0.0-beta1