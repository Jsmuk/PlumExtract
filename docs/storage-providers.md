## Storage Providers

Storage providers (file system, cloud, etc.) are discovered at runtime and loaded via reflection from the `StorageProviders` folder in the output directory.  
They can be used for either source or target and are interchangeable.

### 🔌 Adding a new storage provider plugin

1. **Create a new project**  
   The project should follow the following naming convention:  
   ```
   PlumExtract.Storage.AmazingStorageSystem
   ```

2. **Implement `IBlobStore`**  
   The plugin must implement the `IBlobStore` interface defined in `PlumExtract.Domain`.

3. **Implement constructor that takes in `JsonElement`**  
   Storage providers are 100% responsible for their configuration. They are passed a `JsonElement` directly from the user configuration.  
   They *must* accept this in a constructor with a parameter named `config`.

4. **Reference the plugin project in the main app**  
   In `PlumExtract.csproj`, add a `ProjectReference` like this:

   ```xml
   <ProjectReference Include="..\PlumExtract.Storage.AmazingStorageSystem\PlumExtract.Storage.AmazingStorageSystem.csproj"
                     ReferenceOutputAssembly="false"
                     OutputItemType="StoragePluginDll"
                     CopyLocal="true" />
   ```

5. **Build and run the app**  
   - The plugin project will be built with the solution  
   - Its DLL will be copied to `bin/.../StorageProviders/`  
   - At publish time, it will also be copied to `publish/.../StorageProviders/`  
   - The app will load the plugin at runtime via reflection

6. **Usage**  
   To actually use the storage provider plugin, configure either Source or Target like so:

   ```json
   "Source": {
      "Type": "AmazingStorageSystem",
      "Settings": {
         "Url": "https://amazing.storage.system/container/"
      }
   },
   ```
   The `Type` value must match exactly what is put in the `BlobProvider` attribute. 

---

## 🧪 Example Plugin Structure

```csharp
[BlobProvider("FileSystem")]
public class FileSystemBlobStore : IBlobStore
{
    public FileSystemBlobStore(JsonElement config) { ... }

    public IBlobContainer GetContainer(string name = "") => ...;
}
```

---

## 📁 Output Structure (after build/publish)

```
bin/Debug/net9.0/
├── PlumExtract.exe
├── StorageProviders/
│   └── PlumExtract.Storage.FileSystem.dll
```
