# Resources

All modern games have resources to work with. Resources are things like images, sounds, and models. These resources are usually kept in a separate directory from the source code and can be updated separately without recompiling the source code.

To improve load speeds, MonoGame compiles resources into a binary format, which is then parsed at load time. 


## Adding resources

`Content/Content.mgcb` is a file that contains a list of all the resources that are meant to be compiled. It's a simple text file and can be edited by hand, but it is recommended to use the [MonoGame Content Builder Editor](https://docs.monogame.net/articles/tools/mgcb_editor.html).

This project was built with MonoGame `v3.8.0`. The MGCB Editor `v3.8.0` requires the `.NET 5 Runtime`.
While you can install the MGCB Editor `v3.8.1`, which runs on the `.NET 6 Runtime`, it has not been tested.


1. Install the MGCB Editor

    - *[Recommended]* Install the MonoGame templates for the dotnet CLI

        Windows / macOS / Linux

        ```sh
        dotnet new --install MonoGame.Templates.CSharp
        ```

    - Or you can install the Visual Studio extension
        
        Only since v3.8.1, requires Visual Studio 2022, therefore only Windows / macOS.

        - Windows

            1. Open Visual Studio 2022
            2. Navigate to `Extensions -> Manage Extensions`
            3. Search for `MonoGame`
            4. Install the `MonoGame project templates` extension

        - macOS

            1. Download the extension from [https://github.com/MonoGame/MonoGame/releases/tag/v3.8.1](https://github.com/MonoGame/MonoGame/releases/tag/v3.8.1) (titled `MonoGame.Templates.VSMacExtension_3.8.1.263.mpack`)
            2. Open Visual Studio 2022
            3. Navigate to `Visual Studio -> Extensions...`
            4. Click on `Install from file...`
            5. Select the downloaded file and click `Install`

        For more information visit [https://docs.monogame.net/articles/getting_started/0_getting_started.html](https://docs.monogame.net/articles/getting_started/0_getting_started.html)

2. Open `Content/Content.mgcb` in the MGCB editor

3. Use the New/Add Existing Item/Folder buttons to add resources to the project

4. Save the file


## Building resources

After adding all the resources to `Content/Content.mgcb`, the resources have to be compiled. This should happen automatically when you build the project. If it doesn't or if you want to only compile the resources, you can do it manually.

If you have the MGCB Editor installed, you can use the `Build` button. However, this doesn't reliably work on all platforms. It is recommended (and easier) to use the CLI program.

- *[Recommended]* MonoGame Content Builder *(CLI)*
  
   1. Install the MonoGame Content Builder

       ```sh
       dotnet tool install -g dotnet-mgcb
       ```

    2. Make sure the install location is in your `PATH`

        - Windows: `%USERPROFILE%\.dotnet\tools`

        - macOS / Linux: `$HOME/.dotnet/tools`

    3. Build the resources

       ```sh
       mgcb -r Content/Content.mgcb
       ```

- MonoGame Content Builder Editor *(GUI)*

    1. Open `Content/Content.mgcb` in the MGCB Editor

    2. Click the `Build` button