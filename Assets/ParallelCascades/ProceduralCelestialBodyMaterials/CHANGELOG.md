# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2025-07-30
Editor and Runtime Random Generation

### Added
- Random Procedural Generation methods for asteroid rings, gas giants and stars.
- New sample scene showcasing the runtime random generation of celestial bodies.
- Editor UI Buttons for randomization of Procedural Gas Giant, Procedural Star and Procedural Asteroid Ring inspectors.

### Changed
- Creating new procedural celestial bodies through custom menu commands creates a randomized body by default.

### Fixed
- Fixed angular seam bug in the Asteroid Ring shader (again).
- Missing texture warnings for gradient textures on new body creation.

## [2.0.0] - 2025-07-10
Major update - package structure rework and 3D Skybox Sample scene and URP rendering technique.

### Added
- New welcome window with links to sample scenes and documentation.
- New URP Renderer assets for 3D skybox rendering.
- New sample scene showcasing the 3D skybox effect.
- 3 New gas giant prefabs, used in the 3D skybox sample scene.
- Sample URP Volume profile with Bloom effect to be used as an alternative, or in addition to post-processing glow effect.

### Changed
- Asset format reverted to Assets folder package. This will require uninstalling any previous versions of the package.
- GlowPostFXPass renamed to PostFXGlowPass for consistency, reworked to access the depth and stencil attachment textures for 3D Skybox compatibility.
- Reworked Corona and Gas Giant Glow effects for better performance and visual consistence when scaling their size.

### Fixed
- Visual artifacts at glow effect edges would occur when viewed from great distances.

## [1.1.0] - 2024-12-20

### Added
- New Procedural Gas Giant Glow shader effect.

## [1.0.1] - 2024-12-13

### Changed
- Renamed 'PlanetaryRing.hlsl' to 'AsteroidRing.hlsl' to better fit shader purpose.

### Fixed
- Fixed minor visual bug in asteroid ring shader - angular discontinuity visible as a 1 pixel-wide line.

## [1.0.0] - 2024-12-02

### Added
- Initial release