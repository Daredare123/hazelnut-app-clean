# Hazelnut App iOS Wrapper

This is the Capacitor iOS Wrapper for the Hazelnut web application. 

It is configured to load the live site at `https://hazelnut-app.onrender.com` as a full-screen, native-feeling iOS application with support for push notifications and iPhone safe areas.

## Requirements to Build for iOS
- A Mac computer
- Xcode installed
- Apple Developer Account / Apple ID
- Node.js and npm installed

## Instructions

1. **Transfer Files**: Copy this `HazelnutAppWrapper` folder to your Mac computer.
2. **Install Dependencies**: Open a terminal in the folder and run:
   ```bash
   npm run install-deps
   ```
3. **Add iOS Platform**: Initialize the iOS platform by running:
   ```bash
   npm run add-ios
   ```
4. **Generate App Icons (Optional)**: If you have your icon and splash screen images in an `assets` folder, you can generate the iOS icons by running:
   ```bash
   npm run generate-icons
   ```
5. **Sync Configuration**: To ensure all settings (like URL wrapping and push notifications) are applied to the iOS project, run:
   ```bash
   npm run sync
   ```
6. **Open in Xcode**: Open the generated workspace in Xcode by running:
   ```bash
   npm run open-ios
   ```
   *Note: In Xcode, you will need to sign in with your Apple ID to build the app to a real device or archive it.*

## Features Configured
- **Live Render Site URL Wrapper**: It directly loads `https://hazelnut-app.onrender.com`.
- **Fullscreen / No Safari UI**: Renders within a WKWebView, giving a native look.
- **iPhone Safe Areas**: Support added through `viewport-fit=cover` in your ASP.NET `_Layout.cshtml`.
- **Push Notifications**: Initial configuration ready using the `@capacitor/push-notifications` plugin.
