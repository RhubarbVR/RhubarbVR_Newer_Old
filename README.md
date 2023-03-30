[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![MainBranch](https://github.com/RhubarbVR/RhubarbVR/actions/workflows/BuildAndRunTests.yml/badge.svg)](https://github.com/RhubarbVR/RhubarbVR/actions/workflows/BuildAndRunTests.yml)

<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/RhubarbVR/RhubarbVR">
    <img src="Docs/RhubarbVR.png" alt="Rhubarb-Logo" width="80" height="80">
  </a>

  <h3 align="center">Rhubarb VR</h3>

  <p align="center">
    A Networked VR Engine
    <br />
    <a href="https://github.com/RhubarbVR/RhubarbVR/wiki"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://rhubarbvr.net/">Web Site</a>
    ·
    <a href="https://discord.com/invite/GTQhxeq/">Discord</a>
    ·
    <a href="https://github.com/RhubarbVR/RhubarbVR/issues">Issues</a>
    ·
    <a href="https://github.com/RhubarbVR/RhubarbVR/issues/new?assignees=&labels=&template=bug_report.md&title=Report%20Bug%20Title">Report Bug</a>
    ·
    <a href="https://github.com/RhubarbVR/RhubarbVR/issues/new?assignees=&labels=&template=feature_request.md&title=Feature%20Request%20Title">Make Feature Request</a>
  </p>
</p>

<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#libraries">Libraries</a></li>
      </ul>
    </li>
    <li>
      <a href="#preview">Preview</a>
    </li>
    <!--
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#playing-normally">Playing Normally</a></li>
        <li><a href="#compiling">Compiling</a></li>
      </ul>
    </li>
    -->
    <li><a href="#usage">Usage</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->

## About The Project

RhubarbVR is a networked VR game engine. What does this mean? Well, Rhubarb is a game engine that allows you to add custom shaders and allows you to program in it. It does all of this while synchronizing modifications of the world with everyone in a session so you can collaborate and socialize with people to create amazing things. It allows you to make extensive multiplayer VR games and social experiences.

### Libraries

- [Godot](https://github.com/godotengine/godot)
- [LiteNetLib](https://github.com/RevenantX/LiteNetLib)
- [Assimp](https://bitbucket.org/Starnick/assimpnet/src/master/)
- [BepuPhysics](https://github.com/bepu/bepuphysics2)
- [LibVLCSharp](https://www.nuget.org/packages/LibVLCSharp/3.6.8?_src=template)
- [NAudio](https://github.com/naudio/NAudio)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)

## Preview

![](./Docs/preview.png)

<!-- GETTING STARTED -->

## Getting Started

This is how to run the program and the standard problems you might encounter with trying to start or compile Rhubarb VR.
### Playing Normally

You can get a compiled version of RhubarbVR through this GitHub repository or obtain it from Steam.
But with either of these, you need to the net6 runtime installed.
[Net7 Runtime](https://dotnet.microsoft.com/download/dotnet/7.0/runtime)

### Compiling Windows

1. You will need to download and install [Net7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.202-windows-x64-installer)
2. Clone the repo
   ```sh
   git clone https://github.com/RhubarbVR/RhubarbVR.git
   ```
3. Run Build Helper
   ```
   cd RhubarbVR
   Build.bat
   ```

### Compiling Linux

1. Clone the repo
   ```sh
   git clone https://github.com/RhubarbVR/RhubarbVR.git
   ```
2. Install Net7 SDK Ubuntu 21.04 for [OtherDistros](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu)
   ```sh
   wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   rm packages-microsoft-prod.deb
   sudo apt-get update; \
   sudo apt-get install -y apt-transport-https && \
   sudo apt-get update && \
   sudo apt-get install -y dotnet-sdk-7.0
   ```
3. Run Build Helper
   ```sh
   cd RhubarbVR
   Build.sh
   ```

<!-- USAGE EXAMPLES -->

## Usage

You can use this networked Engine for many things. For example, people can use it for socializing with others, making games, sharing their creativity with others, and educating people.

<!-- ROADMAP -->

## Roadmap

If you want info on where RhubarbVR is going you can join the [DiscordServer](https://discord.com/invite/GTQhxeq/) for info.

1. See the [open issues](https://github.com/RhubarbVR/RhubarbVR//issues) for a list of proposed features (and known issues).
2. See the [Milestones](https://github.com/RhubarbVR/RhubarbVR/milestones) for a list of major features that are actively being implemented.

<!-- CONTRIBUTING -->

## Contributing

Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<!-- LICENSE -->

## License

Distributed under the GPL-3 License. See `LICENSE` for more information.

<!-- CONTACT -->

## Contact

1. Project Link: [https://github.com/RhubarbVR/RhubarbVR](https://github.com/RhubarbVR/RhubarbVR)
2. Website: [https://RhubarbVR.net](https://RhubarbVR.net)
3. DiscordServer: [https://discord.com/invite/GTQhxeq/](https://discord.com/invite/GTQhxeq/)

4. Faolan Rad - Main Developer/Owner - Discord Faolan#0473

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/RhubarbVR/RhubarbVR.svg?style=for-the-badge
[contributors-url]: https://github.com/RhubarbVR/RhubarbVR/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/RhubarbVR/RhubarbVR.svg?style=for-the-badge
[forks-url]: https://github.com/RhubarbVR/RhubarbVR/network/members
[stars-shield]: https://img.shields.io/github/stars/RhubarbVR/RhubarbVR.svg?style=for-the-badge
[stars-url]: https://github.com/RhubarbVR/RhubarbVR/stargazers
[issues-shield]: https://img.shields.io/github/issues/RhubarbVR/RhubarbVR.svg?style=for-the-badge
[issues-url]: https://github.com/RhubarbVR/RhubarbVR/issues
[license-shield]: https://img.shields.io/github/license/RhubarbVR/RhubarbVR.svg?style=for-the-badge
[license-url]: https://github.com/RhubarbVR/RhubarbVR/blob/master/LICENSE
[product-screenshot]: images/screenshot.png
