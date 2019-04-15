# EM-Build-Fetcher
The EM Build Fetcher project is a tool I wrote for QA during my placement year at Ivanti.
When testing, installing the latest build nightly build can be time consuming and often waste a portion of your morning just uninstalling the old version of software (or reverting VM's to previous snapshots) then reinstalling the new one.

This tool solves that issue by monitoring a /build/ folder and if a new update is available, uninstalls the previous version and installs the new one. This way, QA have ready to test software already deployed on their VM's.
Note: The project is targeted heavily on the program QA tested during my placement year and if you would like to use this tool for your own builds, it will need modifying accordingly.

## Getting Started

### Prerequisites
1. C# version 7.0 or higher and .NET core framework
2. Access to the network share that builds are located

### Installing
No setup is required and the project should import into Visual Studio or Ryder without issues.

## Contributing
We are happy to have contributions whether it is for small bug fixes or new pieces of major functionality. To contribute changes, you should first fork the upstream repository to your own GitHub account. You can then add a new remove for upstream and rebase any changes to
make keep up to date with upstream.

`git remote add upstream https://github.com/skdev/EM-Build-Fetcher.git`

## Authors
**Suraj Kumar**

## License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
