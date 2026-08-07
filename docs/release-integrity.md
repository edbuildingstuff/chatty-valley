# Release integrity

Generated from `ChattyValley-0.4.0.zip` by `scripts/generate-release-manifest.ps1`. Every hash below comes from that
artifact's own bytes, so this document is only true for that build. It is regenerated with each release.

- **Archive:** `ChattyValley-0.4.0.zip`, 763.52 MB (800,606,920 bytes)
- **SHA256:** `1b2ea1c01ed2acfb5df9c61730147755bbbbde25ee8eae2ce84e2d9dbf7fd921`

Check any file yourself after unzipping. Hashes are of the extracted file, so this matches what you have on disk:

```powershell
Get-FileHash "Stardew Valley\Mods\ChattyValley\sidecar\ChattyValley.Sidecar.exe" -Algorithm SHA256
```

Each SHA256 below links to its VirusTotal report. VirusTotal keys reports by hash, so a link resolves
whether or not anyone has scanned that exact file yet; an unscanned file shows as not found until
someone uploads it, and the link starts working from that moment.

## The launcher

The one file people ask about. It is a thin native shim that starts the .NET runtime; the logic lives in
the managed assemblies below it.

| File | Size | SHA256 |
|---|---|---|
| `sidecar/ChattyValley.Sidecar.exe` | 159 KB | [`574b86f4fe53e581...`](https://www.virustotal.com/gui/file/574b86f4fe53e581a624e90ac1f1f7fc56ccaaf2821191a97a067b79eeb6d94d) |

## llama.cpp inference, native

25 files, 24 unique binaries. Four builds ship per library, one per CPU instruction set, and your
machine loads exactly one set at runtime. They come from the `LLamaSharp.Backend.Cpu` NuGet package and
are not built by this project.

**Every one of them is byte-identical to that published package.** You can check this without trusting
us: download `llamasharp.backend.cpu.0.27.0.nupkg` from nuget.org, open it as a zip, and hash the files
under `runtimes/win-x64/native/`. They match the table below exactly.

That matters because **antivirus engines do sometimes flag these libraries.** They are large, heavily
optimised, hand-vectorised native code that allocates a lot of memory and JITs compute kernels, which is
a shape that trips machine-learning heuristics. When it happens it is usually a single engine out of
roughly seventy, with a generic verdict name rather than a named malware family. Because the binaries are
unmodified upstream builds, any such verdict is a statement about the standard llama.cpp Windows release
that thousands of projects ship, not about anything compiled here. Check the links, and weigh one
detection out of seventy accordingly.

| File | Size | SHA256 |
|---|---|---|
| `sidecar/runtimes/win-x64/native/avx/ggml.dll` | 66 KB | [`90653a391d5cc6af...`](https://www.virustotal.com/gui/file/90653a391d5cc6af47b52b23ef539a057122faabcf4da362cba32115220ecf40) |
| `sidecar/runtimes/win-x64/native/avx/ggml-base.dll` | 602 KB | [`7b110a7049afb7b7...`](https://www.virustotal.com/gui/file/7b110a7049afb7b7c653af4f8f3c023e59995b673df8468db7620a9ff5d9a4d5) |
| `sidecar/runtimes/win-x64/native/avx/ggml-cpu.dll` | 802 KB | [`447502e055c0df2f...`](https://www.virustotal.com/gui/file/447502e055c0df2ffd7ceba6e800e1608029bd1d6c760e5a1b29b2f6d9671a72) |
| `sidecar/runtimes/win-x64/native/avx/llama.dll` | 1.96 MB | [`43465972ebdc01fd...`](https://www.virustotal.com/gui/file/43465972ebdc01fddb0b09feb613230a2844e6658e0b2da88025ddfdd51deddb) |
| `sidecar/runtimes/win-x64/native/avx/mtmd.dll` | 784 KB | [`978c6423d378e93a...`](https://www.virustotal.com/gui/file/978c6423d378e93a5436c9162cf8f8cc1694ce1d8821b17e039201f91285c9f9) |
| `sidecar/runtimes/win-x64/native/avx2/ggml.dll` | 66 KB | [`44ec41894eb611c9...`](https://www.virustotal.com/gui/file/44ec41894eb611c9bb86055cedb6759603fcb6a9dc994abccb15f08a031b486a) |
| `sidecar/runtimes/win-x64/native/avx2/ggml-base.dll` | 602 KB | [`09835b5faec3acc7...`](https://www.virustotal.com/gui/file/09835b5faec3acc7e88f0ddee1ddf8a303108c9664fc581c6b5bff03f1032353) |
| `sidecar/runtimes/win-x64/native/avx2/ggml-cpu.dll` | 858 KB | [`5c579f09d7b4f782...`](https://www.virustotal.com/gui/file/5c579f09d7b4f782c534b03f5962ff82cdddf9374aa443175f9db3cbc86b7b3c) |
| `sidecar/runtimes/win-x64/native/avx2/llama.dll` | 1.96 MB | [`1aa8f6f65386b7b9...`](https://www.virustotal.com/gui/file/1aa8f6f65386b7b977f9a0bb631d6d7e2a30d3beb3f5747e80e1b114f4885fb8) |
| `sidecar/runtimes/win-x64/native/avx2/mtmd.dll` | 784 KB | [`5da6779429096118...`](https://www.virustotal.com/gui/file/5da6779429096118c15ccc134f0309bb0df6f0a8c010b16e74475285288e3fb6) |
| `sidecar/runtimes/win-x64/native/avx512/ggml.dll` | 66 KB | [`23b4bcf921c6e77d...`](https://www.virustotal.com/gui/file/23b4bcf921c6e77d6e0a7864f740f7eb4aec2cb946453f46b8fa5d337460ad8f) |
| `sidecar/runtimes/win-x64/native/avx512/ggml-base.dll` | 602 KB | [`6a45172a836430e3...`](https://www.virustotal.com/gui/file/6a45172a836430e372adb31039dd48a94fd5f2a2c023c0f2f7629e0e0faa8fe5) |
| `sidecar/runtimes/win-x64/native/avx512/ggml-cpu.dll` | 957 KB | [`9bbd41bbda9904f7...`](https://www.virustotal.com/gui/file/9bbd41bbda9904f72194edccffe91068f56e5b6d49d874a556db08b320e222c7) |
| `sidecar/runtimes/win-x64/native/avx512/llama.dll` | 1.96 MB | [`1aa8f6f65386b7b9...`](https://www.virustotal.com/gui/file/1aa8f6f65386b7b977f9a0bb631d6d7e2a30d3beb3f5747e80e1b114f4885fb8) |
| `sidecar/runtimes/win-x64/native/avx512/mtmd.dll` | 784 KB | [`ca9ddab5416c5558...`](https://www.virustotal.com/gui/file/ca9ddab5416c5558d98585fbdb60e0fcb52e1ee2e5395b1ac8848c77e392b3fe) |
| `sidecar/runtimes/win-x64/native/noavx/ggml.dll` | 66 KB | [`905ec5ea9db9bd1e...`](https://www.virustotal.com/gui/file/905ec5ea9db9bd1e795cf2cb4f7f9399c50e82a74379efe75815de7715da45cf) |
| `sidecar/runtimes/win-x64/native/noavx/ggml-base.dll` | 602 KB | [`57803fba605d6e3e...`](https://www.virustotal.com/gui/file/57803fba605d6e3eae4bfc48bf6b120c03ddd3926ad5a0e3bb2b11598b20f598) |
| `sidecar/runtimes/win-x64/native/noavx/ggml-cpu.dll` | 692 KB | [`bb3d65715350df4b...`](https://www.virustotal.com/gui/file/bb3d65715350df4b9f2d605367e7063e27c40146330e6ed9807598c41aec08a5) |
| `sidecar/runtimes/win-x64/native/noavx/llama.dll` | 1.96 MB | [`6e18361b4fdce035...`](https://www.virustotal.com/gui/file/6e18361b4fdce03588c19cc21a818ffcb4d3326d3debaa898af4cc27fd10c877) |
| `sidecar/runtimes/win-x64/native/noavx/mtmd.dll` | 784 KB | [`c50e84ef123adc01...`](https://www.virustotal.com/gui/file/c50e84ef123adc015e80d4e3909f83a85b1b35be335a537eaec7b6e13d0a525e) |
| `sidecar/runtimes/win-x64/native/vulkan/ggml.dll` | 66 KB | [`7465a5e0b304255a...`](https://www.virustotal.com/gui/file/7465a5e0b304255a62fb2e5d01c106cc39fc0f4362bfd99ff99acb1278cacccc) |
| `sidecar/runtimes/win-x64/native/vulkan/ggml-base.dll` | 602 KB | [`7e838ba129672c59...`](https://www.virustotal.com/gui/file/7e838ba129672c59ad728f56fc5685cadefda56ad50a8fcbccbb9c851ed90b4a) |
| `sidecar/runtimes/win-x64/native/vulkan/ggml-vulkan.dll` | 59.11 MB | [`515171efa8556572...`](https://www.virustotal.com/gui/file/515171efa8556572795ea37f78193ef56d3dd5079d3d221979be89c107938038) |
| `sidecar/runtimes/win-x64/native/vulkan/llama.dll` | 1.96 MB | [`e9f3127597085ce9...`](https://www.virustotal.com/gui/file/e9f3127597085ce9d9a0e49e4bd1f62efe35624c76f0964e802dc7c60b5156bb) |
| `sidecar/runtimes/win-x64/native/vulkan/mtmd.dll` | 784 KB | [`de645e6110e6646d...`](https://www.virustotal.com/gui/file/de645e6110e6646d91e7a9445c8a2c6715564adff1d0886a165eb559624a9015) |

## This project's own code, managed

Small because they are just the mod logic. No antivirus engine holds signatures for a single developer's
assemblies, so a clean result here proves less than reading the source, which is the point of this repo.

| File | Size | SHA256 |
|---|---|---|
| `ChattyValley.Core.dll` | 39 KB | [`d3d878f93b33f262...`](https://www.virustotal.com/gui/file/d3d878f93b33f262eb5a0cb98fd55002f62ae6deac0f059913496b08213e1f99) |
| `ChattyValley.Mod.dll` | 56 KB | [`d83d7e8d6fe4575c...`](https://www.virustotal.com/gui/file/d83d7e8d6fe4575c1110805cdc10c4600f38123f48a2bf9fa74dd4fc010e65cd) |
| `sidecar/ChattyValley.Core.dll` | 39 KB | [`d3d878f93b33f262...`](https://www.virustotal.com/gui/file/d3d878f93b33f262eb5a0cb98fd55002f62ae6deac0f059913496b08213e1f99) |
| `sidecar/ChattyValley.Runtime.dll` | 14 KB | [`42bcdd2cf5ec63ac...`](https://www.virustotal.com/gui/file/42bcdd2cf5ec63acb580401753341526e263a6f82c63162d5e37175337fa773e) |
| `sidecar/ChattyValley.Sidecar.dll` | 19 KB | [`d613c35a7c97f6cd...`](https://www.virustotal.com/gui/file/d613c35a7c97f6cd27d2d901b8697dd7bfc84f3caacaf23ea9a7740c6289d692) |

## Model weights

Data, not code. Nothing executes these; `llama.cpp` reads them as tensors. Listed for completeness and so
you can confirm the download is intact. The base model exceeds VirusTotal's upload limit, so these carry
hashes rather than scan links.

| File | Size | SHA256 |
|---|---|---|
| `assets/LFM2.5-1.2B-Instruct-Q4_K_M.gguf` | 697.04 MB | `b1b3de114215d9507409a662a501a631095a479a419584e8a2ded6304b19b4f5` |
| `assets/linus-12b-v8dpo2-lora-f16.gguf` | 21.20 MB | `d6371a5c2ce12a9c752989f52ffa6956475bbe8ac54c7800112d0d525eeb7610` |

## Microsoft .NET runtime

198 files, 80.05 MB. The sidecar ships self-contained so players need no .NET install. These are Microsoft's,
unmodified and Microsoft-signed; scanning them tells you about Microsoft rather than about this mod. Listed
in full anyway, because "every executable file" should mean every one.

<details><summary>Show all 198 runtime files</summary>

| File | Size | SHA256 |
|---|---|---|
| `sidecar/clretwrc.dll` | 310 KB | [`2dba4804094fb908...`](https://www.virustotal.com/gui/file/2dba4804094fb9080ad7954d6664c168f85c244691198046faf9d33e12e70107) |
| `sidecar/clrgc.dll` | 657 KB | [`ca2d50a39e14bbe2...`](https://www.virustotal.com/gui/file/ca2d50a39e14bbe2666d53789317e87daf65af8b2870ab400b352bb2734aec33) |
| `sidecar/clrgcexp.dll` | 694 KB | [`e683eb9c26c7cf77...`](https://www.virustotal.com/gui/file/e683eb9c26c7cf77ae7f27369472d107e7426d6421a5e6aba24e58a35630f9e4) |
| `sidecar/clrjit.dll` | 2.00 MB | [`9a74076395f6e7bc...`](https://www.virustotal.com/gui/file/9a74076395f6e7bcbb932c8b6eb412d1fbbff43b954c49f9d5015e467d8402e9) |
| `sidecar/CommunityToolkit.HighPerformance.dll` | 150 KB | [`196607c6d4d704b5...`](https://www.virustotal.com/gui/file/196607c6d4d704b5ee95c80f8e3cd799595b7d8a08108701894c211b138d2839) |
| `sidecar/coreclr.dll` | 4.40 MB | [`58859f85a30cc713...`](https://www.virustotal.com/gui/file/58859f85a30cc71313b281898e7cfbdbb9eccb95ae2a3f865329efd47ebf31bb) |
| `sidecar/hostfxr.dll` | 371 KB | [`d1012d5b8ff1329d...`](https://www.virustotal.com/gui/file/d1012d5b8ff1329d5baa6d58aa02c4277acbe58cfd1dc5f10d7e6bb8e9a0d94a) |
| `sidecar/hostpolicy.dll` | 370 KB | [`e9c723bf674ef7f6...`](https://www.virustotal.com/gui/file/e9c723bf674ef7f6e6c1cc5d4ee694715b864f480c56baaf354b8cf7c9b40b87) |
| `sidecar/LLamaSharp.dll` | 283 KB | [`559fd41e6a9757cc...`](https://www.virustotal.com/gui/file/559fd41e6a9757ccd7aaec61255ecde33bc0294894510e10b94d095e02ad7c10) |
| `sidecar/Microsoft.Bcl.AsyncInterfaces.dll` | 20 KB | [`e925fe423d376b8c...`](https://www.virustotal.com/gui/file/e925fe423d376b8c795c17ef57ef61a749f3bd34f044e72b2243103feadac1ec) |
| `sidecar/Microsoft.Bcl.Memory.dll` | 16 KB | [`d844a2e31b7a0abc...`](https://www.virustotal.com/gui/file/d844a2e31b7a0abc10911608e0a819523aa055774a5b46e8e12de14cd5a44b32) |
| `sidecar/Microsoft.CSharp.dll` | 958 KB | [`e140acec05bde933...`](https://www.virustotal.com/gui/file/e140acec05bde933bf7075d501b6adb357b8e1c53c0519827731904fb2a3420f) |
| `sidecar/Microsoft.DiaSymReader.Native.amd64.dll` | 2.09 MB | [`315a61c47c41b8d4...`](https://www.virustotal.com/gui/file/315a61c47c41b8d4c815f4274bb6584c1d24b4640c7eedc354ed5b03a9a1361e) |
| `sidecar/Microsoft.Extensions.AI.Abstractions.dll` | 654 KB | [`688fea67d5323883...`](https://www.virustotal.com/gui/file/688fea67d5323883bd10086a83a6aac01f791830dcebbefb03515c9002a9d8f3) |
| `sidecar/Microsoft.Extensions.DependencyInjection.Abstractions.dll` | 65 KB | [`842fc19802a761e4...`](https://www.virustotal.com/gui/file/842fc19802a761e49c65b0e90b2c68bc6868b989145da5ac6ca30b766e2658f2) |
| `sidecar/Microsoft.Extensions.Logging.Abstractions.dll` | 66 KB | [`03e916b9c5f91717...`](https://www.virustotal.com/gui/file/03e916b9c5f91717b67c790325d4e5dd2debe81d8e403928806ba1d430ea7147) |
| `sidecar/Microsoft.VisualBasic.Core.dll` | 1.14 MB | [`897f3d3938b03156...`](https://www.virustotal.com/gui/file/897f3d3938b03156202acf780bbe156227e034785bd17305b17657c911c62203) |
| `sidecar/Microsoft.VisualBasic.dll` | 17 KB | [`d58ecdaef545a533...`](https://www.virustotal.com/gui/file/d58ecdaef545a533ecd96f4bf2d1e73f5f0fa7615f37e68859ee19a6ecdd1e31) |
| `sidecar/Microsoft.Win32.Primitives.dll` | 15 KB | [`1c6d1ee2a3d8a82a...`](https://www.virustotal.com/gui/file/1c6d1ee2a3d8a82a097d5e1eda84fbddca57fbe5fc426f0f39aa9d2b486bc3dc) |
| `sidecar/Microsoft.Win32.Registry.dll` | 114 KB | [`eb249fb10eb8687d...`](https://www.virustotal.com/gui/file/eb249fb10eb8687d3505fd9276480fb9a0c13dc975ce6bcc35e2e2e723e8d559) |
| `sidecar/mscordaccore.dll` | 1.29 MB | [`01625b0382d7ccab...`](https://www.virustotal.com/gui/file/01625b0382d7ccab664ddb0997de2bb66e059fc547061f3d830914d6229a07f6) |
| `sidecar/mscordaccore_amd64_amd64_10.0.1026.32716.dll` | 1.29 MB | [`01625b0382d7ccab...`](https://www.virustotal.com/gui/file/01625b0382d7ccab664ddb0997de2bb66e059fc547061f3d830914d6229a07f6) |
| `sidecar/mscordbi.dll` | 1.18 MB | [`35aa0553836e0343...`](https://www.virustotal.com/gui/file/35aa0553836e034308189a97ed55b6722636960cb201e252c20bd49f8ac65ead) |
| `sidecar/mscorlib.dll` | 58 KB | [`ec2fdd1e7113428e...`](https://www.virustotal.com/gui/file/ec2fdd1e7113428e9c2e0aa8bea39d54f4dafc2102434aa0a13c31f7ed1a4fb2) |
| `sidecar/mscorrc.dll` | 131 KB | [`331ce4f5bd1cfd59...`](https://www.virustotal.com/gui/file/331ce4f5bd1cfd59a4e1537ca62d5cc5693675b8c82a42125ab24e8d5104d1a0) |
| `sidecar/msquic.dll` | 512 KB | [`99daf179ee2dfb5c...`](https://www.virustotal.com/gui/file/99daf179ee2dfb5c36493543647b64fc58a24aba0b16d5e9232b06003fbc9054) |
| `sidecar/netstandard.dll` | 98 KB | [`f9078068add6f694...`](https://www.virustotal.com/gui/file/f9078068add6f69455772c526a69a134ae520199eb1c5513f7d80a871fc09851) |
| `sidecar/System.AppContext.dll` | 15 KB | [`1776b3c8d8e40ce4...`](https://www.virustotal.com/gui/file/1776b3c8d8e40ce4baee66904870272305546233cab9c84bccf95cbe685b7887) |
| `sidecar/System.Buffers.dll` | 15 KB | [`4a5e9e4b8d439bfe...`](https://www.virustotal.com/gui/file/4a5e9e4b8d439bfede0435440e252000a5f2e4a5d4b915f243e909ca61bc528b) |
| `sidecar/System.CodeDom.dll` | 180 KB | [`36d70ccbb49f8fd3...`](https://www.virustotal.com/gui/file/36d70ccbb49f8fd3cb60c32d2cd1db0ddfc505066f0630040e567f170f80287e) |
| `sidecar/System.Collections.Concurrent.dll` | 226 KB | [`5a4b728246443eb3...`](https://www.virustotal.com/gui/file/5a4b728246443eb3b4c0a16d9b3d04bc0bce60b7a1998fa4a75716358cc572ff) |
| `sidecar/System.Collections.dll` | 290 KB | [`fc52be0abceac360...`](https://www.virustotal.com/gui/file/fc52be0abceac360478ffffce876c99997b4af56fc36fbf07cfc38ac5b7fdd7f) |
| `sidecar/System.Collections.Immutable.dll` | 954 KB | [`c1537a4f73a7d9fa...`](https://www.virustotal.com/gui/file/c1537a4f73a7d9fa028b127a5dd7d4f0e839c75c22c8e895722283a8aadf6028) |
| `sidecar/System.Collections.NonGeneric.dll` | 102 KB | [`b64b8f2acaf773bc...`](https://www.virustotal.com/gui/file/b64b8f2acaf773bc08e20e3a43d562a15a98d9a8c19f3e5db292dfe4fe21cf38) |
| `sidecar/System.Collections.Specialized.dll` | 102 KB | [`8a1cf61a6cd07944...`](https://www.virustotal.com/gui/file/8a1cf61a6cd079444ae790123ffc90bcd8d369ec5aed0f2f95d159a6479690b3) |
| `sidecar/System.ComponentModel.Annotations.dll` | 194 KB | [`fe7b216e9ac726cb...`](https://www.virustotal.com/gui/file/fe7b216e9ac726cbcc31dcdaaf99f74918ad3c255100e3571f635e46f3fe5a36) |
| `sidecar/System.ComponentModel.DataAnnotations.dll` | 16 KB | [`ca01349cd2181d50...`](https://www.virustotal.com/gui/file/ca01349cd2181d50d15bc476d542d23e62a2771d4bf9d467ad8d4f61563f5754) |
| `sidecar/System.ComponentModel.dll` | 30 KB | [`23c6a24f67244c64...`](https://www.virustotal.com/gui/file/23c6a24f67244c647a614af3249181dc68ad02acc3ebd3cdedf62b2df8da503a) |
| `sidecar/System.ComponentModel.EventBasedAsync.dll` | 46 KB | [`d0e3554dc311c750...`](https://www.virustotal.com/gui/file/d0e3554dc311c75014a9c7e9cf8137d5881d6f7a8c3b739fd9e85fec2be13164) |
| `sidecar/System.ComponentModel.Primitives.dll` | 78 KB | [`52e0988e01955d09...`](https://www.virustotal.com/gui/file/52e0988e01955d09170339429cede1d578690418b8851e19ddbba7d22a6490f7) |
| `sidecar/System.ComponentModel.TypeConverter.dll` | 750 KB | [`f41fb0c7373b94e0...`](https://www.virustotal.com/gui/file/f41fb0c7373b94e082d41a49a02f95ac6da92114aa712dcd8eddcfd27fbfd592) |
| `sidecar/System.Configuration.dll` | 19 KB | [`24d363fccb6cef1b...`](https://www.virustotal.com/gui/file/24d363fccb6cef1b612ff6c3d9e2524c766713aaa3b5762aed169cc41c5caf30) |
| `sidecar/System.Console.dll` | 166 KB | [`d6c46a6825879377...`](https://www.virustotal.com/gui/file/d6c46a682587937775464bb1a3850a9d3e11408440731fefb07d298cd6d7a99c) |
| `sidecar/System.Core.dll` | 23 KB | [`f8732fb6c15b84e7...`](https://www.virustotal.com/gui/file/f8732fb6c15b84e7c10026d7d01ac6c7772b8e8fecebe6b0a6b6c4776b3d763d) |
| `sidecar/System.Data.Common.dll` | 2.64 MB | [`ea52008bc561c36f...`](https://www.virustotal.com/gui/file/ea52008bc561c36fb8b78aa42ab760fe4d00d8f05ed2f953a2f2432fa5712bdb) |
| `sidecar/System.Data.DataSetExtensions.dll` | 15 KB | [`ec6614f5a6ebd420...`](https://www.virustotal.com/gui/file/ec6614f5a6ebd42061848ad3475adb2e1717815309a7c5c4e87b5590108c2f98) |
| `sidecar/System.Data.dll` | 25 KB | [`36fabeaaf88affe7...`](https://www.virustotal.com/gui/file/36fabeaaf88affe70bd3cfffc82ed936d4c94e1b43cae48b6dbe6e98c72af119) |
| `sidecar/System.Diagnostics.Contracts.dll` | 16 KB | [`3ef6f93deeca5b85...`](https://www.virustotal.com/gui/file/3ef6f93deeca5b85edb693194e5b4529220420a0c6ca0f3a9d1a2ac0826ac0a0) |
| `sidecar/System.Diagnostics.Debug.dll` | 15 KB | [`df925984086638f1...`](https://www.virustotal.com/gui/file/df925984086638f1046e1c5bd695107c7a3397e43767c01248a36360e4d6fa9b) |
| `sidecar/System.Diagnostics.DiagnosticSource.dll` | 490 KB | [`aa6ed09aa5b7588d...`](https://www.virustotal.com/gui/file/aa6ed09aa5b7588d8cdcf9d88c9c5317be654c06cdb08d161b61c607dc09c14f) |
| `sidecar/System.Diagnostics.FileVersionInfo.dll` | 46 KB | [`ff481f04e205c815...`](https://www.virustotal.com/gui/file/ff481f04e205c815ddbb48e622515b426e6614c9ad968ed4f3db42a63420a3bb) |
| `sidecar/System.Diagnostics.Process.dll` | 322 KB | [`f2ad68f64541926e...`](https://www.virustotal.com/gui/file/f2ad68f64541926edb0ea5f040775a49991ac0f24baf98b2de62ab038b69c895) |
| `sidecar/System.Diagnostics.StackTrace.dll` | 46 KB | [`2436e12f3c710100...`](https://www.virustotal.com/gui/file/2436e12f3c710100f38b9404fb3893c3da3ee295641f5b4be442a4aa0deea8fa) |
| `sidecar/System.Diagnostics.TextWriterTraceListener.dll` | 66 KB | [`b5d9e94d097856e4...`](https://www.virustotal.com/gui/file/b5d9e94d097856e49a5c61a87b442d42f2f70d76d7ce7b7e897f594b5194558f) |
| `sidecar/System.Diagnostics.Tools.dll` | 15 KB | [`d0712e1303d134cb...`](https://www.virustotal.com/gui/file/d0712e1303d134cb75f4425a2f764390d965dec6b41195600bfae18f97b0e0f5) |
| `sidecar/System.Diagnostics.TraceSource.dll` | 138 KB | [`08c9b12b71be630b...`](https://www.virustotal.com/gui/file/08c9b12b71be630bd29fbca96af28e790962efc8f2006363788ac592b1b97886) |
| `sidecar/System.Diagnostics.Tracing.dll` | 16 KB | [`1e531d4462aa8134...`](https://www.virustotal.com/gui/file/1e531d4462aa8134d81405ea745153395e068504704f84f447d50b4586dcc213) |
| `sidecar/System.dll` | 49 KB | [`f2bdfd2096aeadf6...`](https://www.virustotal.com/gui/file/f2bdfd2096aeadf69ea2c9e747f0ca2842127cde1770d1643bf13bfc70649361) |
| `sidecar/System.Drawing.dll` | 20 KB | [`2d6706f2b1b885e3...`](https://www.virustotal.com/gui/file/2d6706f2b1b885e31fdce57eef38ed308cdd1563877d51bcfecc19a9268125b4) |
| `sidecar/System.Drawing.Primitives.dll` | 130 KB | [`d55e07078df7e78a...`](https://www.virustotal.com/gui/file/d55e07078df7e78a52992e8123f27ad01e632d6ed0ae12c08f099af33b36dbdb) |
| `sidecar/System.Dynamic.Runtime.dll` | 16 KB | [`eaa819f663bef9cb...`](https://www.virustotal.com/gui/file/eaa819f663bef9cb04890c9797aa7b0110bca064e62ea47aa221548707baae5b) |
| `sidecar/System.Formats.Asn1.dll` | 234 KB | [`36d5622b54e87a34...`](https://www.virustotal.com/gui/file/36d5622b54e87a3444af5e26e5b69e8cb048f41cceb66583689e555232defed3) |
| `sidecar/System.Formats.Tar.dll` | 266 KB | [`37916824b9f130f7...`](https://www.virustotal.com/gui/file/37916824b9f130f7e924be01b1bb1a479636972c3b27af307b26592597fad2b3) |
| `sidecar/System.Globalization.Calendars.dll` | 15 KB | [`17934d74ca8ae2ad...`](https://www.virustotal.com/gui/file/17934d74ca8ae2adf890be3bbfdffd02c4d74d090010d808b4403e721f33a67b) |
| `sidecar/System.Globalization.dll` | 15 KB | [`3ae6f3c344796033...`](https://www.virustotal.com/gui/file/3ae6f3c344796033247f710612f2944688def8c79ba80b4846b8b14c984f844f) |
| `sidecar/System.Globalization.Extensions.dll` | 15 KB | [`aeb5d04073661df2...`](https://www.virustotal.com/gui/file/aeb5d04073661df29bd5ec68f5a5786df4ca620ba168a9aa58b91ab6ad316fe1) |
| `sidecar/System.Interactive.Async.dll` | 359 KB | [`ffac755159ef51c4...`](https://www.virustotal.com/gui/file/ffac755159ef51c4b119a6d68650d0e8e5d392d54bcfd27513cb30aa45393f0d) |
| `sidecar/System.IO.Compression.Brotli.dll` | 82 KB | [`3b770188543233b9...`](https://www.virustotal.com/gui/file/3b770188543233b988f44f5fe48e92ffb2c81c63978b89ff8da574edaf43e60c) |
| `sidecar/System.IO.Compression.dll` | 410 KB | [`fcc979a99b747680...`](https://www.virustotal.com/gui/file/fcc979a99b747680a81f5eaf71add66f3876588e59c90edf826c37e525d4f0b2) |
| `sidecar/System.IO.Compression.FileSystem.dll` | 15 KB | [`aaea6b18581abe92...`](https://www.virustotal.com/gui/file/aaea6b18581abe925d9ee38749773363d73c8c033c63b284af3faa228430ade9) |
| `sidecar/System.IO.Compression.Native.dll` | 916 KB | [`a61a917b3df5d74a...`](https://www.virustotal.com/gui/file/a61a917b3df5d74af1f997a82c0137076fe0fd3c3e0038423e3690924fb10c67) |
| `sidecar/System.IO.Compression.ZipFile.dll` | 98 KB | [`7634d9c46f688033...`](https://www.virustotal.com/gui/file/7634d9c46f688033a8a7cc6bee22770b032e2e611da5cc1d57f3d63984774ff0) |
| `sidecar/System.IO.dll` | 15 KB | [`7b40c8b70bf1b619...`](https://www.virustotal.com/gui/file/7b40c8b70bf1b61924b4fade11d4325151debc2f5733f32fbe3409621914c802) |
| `sidecar/System.IO.FileSystem.AccessControl.dll` | 102 KB | [`1e8dca7e8cec6bfa...`](https://www.virustotal.com/gui/file/1e8dca7e8cec6bfa9c1baf966550ef4b2482bae6439d6d2ae55dadc265ff11a0) |
| `sidecar/System.IO.FileSystem.dll` | 15 KB | [`ff3fe903e942724e...`](https://www.virustotal.com/gui/file/ff3fe903e942724ebb71f363d973cb4225b4f14d0e05ebf73d0b30e0f46c1486) |
| `sidecar/System.IO.FileSystem.DriveInfo.dll` | 54 KB | [`375e715d0b7af1b7...`](https://www.virustotal.com/gui/file/375e715d0b7af1b756fcd06c7912791bf714be03832ecf0d9c4d7f885c98cc3a) |
| `sidecar/System.IO.FileSystem.Primitives.dll` | 15 KB | [`6d025a5ee98a81ac...`](https://www.virustotal.com/gui/file/6d025a5ee98a81ac276250eea1a2677ea0792c75f9d1a9807c8a775e75b100ce) |
| `sidecar/System.IO.FileSystem.Watcher.dll` | 86 KB | [`791ffd9ad477481c...`](https://www.virustotal.com/gui/file/791ffd9ad477481c3ca2d255b325a991d03aec46b6c51241e96247379efc37dd) |
| `sidecar/System.IO.IsolatedStorage.dll` | 86 KB | [`b2150acfade52275...`](https://www.virustotal.com/gui/file/b2150acfade522755ed15027112541bb4655107bb85122507c03d9e879e682a1) |
| `sidecar/System.IO.MemoryMappedFiles.dll` | 82 KB | [`de6de29fd36c5217...`](https://www.virustotal.com/gui/file/de6de29fd36c52173c9a82c059c430cc6ca3ffb4a61aabf896c8ff56e6eed3d4) |
| `sidecar/System.IO.Pipelines.dll` | 186 KB | [`427b8641bbcce1e3...`](https://www.virustotal.com/gui/file/427b8641bbcce1e355c8b72415bd8ef10c91eb4171c77fae001674df495cb5f8) |
| `sidecar/System.IO.Pipes.AccessControl.dll` | 15 KB | [`dac370e76771c384...`](https://www.virustotal.com/gui/file/dac370e76771c3843c8238fe91562f09d8f28e318a6da1b05f76afbf40eff8cf) |
| `sidecar/System.IO.Pipes.dll` | 162 KB | [`ea4e651e0c0e18ec...`](https://www.virustotal.com/gui/file/ea4e651e0c0e18ecc380013769d1864f9777b2d28964d8dab24c37ff6ed59206) |
| `sidecar/System.IO.UnmanagedMemoryStream.dll` | 15 KB | [`67eb115d743c83ca...`](https://www.virustotal.com/gui/file/67eb115d743c83cade373af3fdf7d6d4e4fdd6e26105b21e8968727821f707e0) |
| `sidecar/System.Linq.Async.dll` | 1.13 MB | [`9023bdc38c02823f...`](https://www.virustotal.com/gui/file/9023bdc38c02823fd86a2100350455ec0521fbef0267ad7e62ce38b3c44df608) |
| `sidecar/System.Linq.AsyncEnumerable.dll` | 1.23 MB | [`f62a69be4b87aefd...`](https://www.virustotal.com/gui/file/f62a69be4b87aefd74ca7080f4d1fac332a8b81640ec40e1c61ed3b2133aa1bc) |
| `sidecar/System.Linq.dll` | 682 KB | [`4a5d41332ed37f7e...`](https://www.virustotal.com/gui/file/4a5d41332ed37f7e195503ebd3f478da2ad0008cb8a9f85739b53d2df7d33d46) |
| `sidecar/System.Linq.Expressions.dll` | 3.47 MB | [`75efc145c24374e4...`](https://www.virustotal.com/gui/file/75efc145c24374e41b3523b4bbf06e5fa415cb63c6bc4da717aa58440871c0c3) |
| `sidecar/System.Linq.Parallel.dll` | 758 KB | [`ce2d359ab186c595...`](https://www.virustotal.com/gui/file/ce2d359ab186c59558967d3442e705558e0d13f814ec39f40c16b9076385652f) |
| `sidecar/System.Linq.Queryable.dll` | 178 KB | [`d7d0418293d01388...`](https://www.virustotal.com/gui/file/d7d0418293d01388701c4cc557223daa2e88e1afede9b67445f9058acaebe675) |
| `sidecar/System.Management.dll` | 305 KB | [`b2d6e7e991dc9ef1...`](https://www.virustotal.com/gui/file/b2d6e7e991dc9ef154b29f4966f04fd8ed4ebb2c1d1242ea1d5f3e90f8ae5143) |
| `sidecar/System.Memory.dll` | 158 KB | [`03c2fe17dbd4e1cd...`](https://www.virustotal.com/gui/file/03c2fe17dbd4e1cd709f6da1ff221eafaf3f076d23f8fc62ddff5302c152e97d) |
| `sidecar/System.Net.dll` | 17 KB | [`115811026128a2bd...`](https://www.virustotal.com/gui/file/115811026128a2bdc84ead6c4f6a7b35b296d092420fa5794d3c07402ab9e9d4) |
| `sidecar/System.Net.Http.dll` | 1.67 MB | [`277ee31df794738d...`](https://www.virustotal.com/gui/file/277ee31df794738d802c9dca109565ee1f549e733741af03b17f907208d13538) |
| `sidecar/System.Net.Http.Json.dll` | 126 KB | [`93d66d4f208cd3f3...`](https://www.virustotal.com/gui/file/93d66d4f208cd3f36c0c52814f62c3a06e28e909f551e6f6f01ce66db4282148) |
| `sidecar/System.Net.HttpListener.dll` | 522 KB | [`53d55125f448882e...`](https://www.virustotal.com/gui/file/53d55125f448882ec2d002cce5726b5825a22847642f8c659cba0ae1bd2b97d1) |
| `sidecar/System.Net.Mail.dll` | 470 KB | [`01d3bb4d99659b85...`](https://www.virustotal.com/gui/file/01d3bb4d99659b85e6af59ca33f78fd7c4f5b5bc540be69b384f086687b04d3d) |
| `sidecar/System.Net.NameResolution.dll` | 122 KB | [`ea47fa390901b0af...`](https://www.virustotal.com/gui/file/ea47fa390901b0afe369711065f79d86f1cf01a307746a82c4dfd3bb94e156a0) |
| `sidecar/System.Net.NetworkInformation.dll` | 154 KB | [`7a520cbc73fab570...`](https://www.virustotal.com/gui/file/7a520cbc73fab570bc069717d0b73efc03cc100196ff42137dba24066eeedb83) |
| `sidecar/System.Net.Ping.dll` | 90 KB | [`927c63c02f6ac45d...`](https://www.virustotal.com/gui/file/927c63c02f6ac45d190c8994a1890c1e45d1144474d48d436fa101eba46e3ddc) |
| `sidecar/System.Net.Primitives.dll` | 206 KB | [`e0be3d784a346ffa...`](https://www.virustotal.com/gui/file/e0be3d784a346ffa419a5b4f6aba91d20a909a122b01ce331c4ac1560bf93c3c) |
| `sidecar/System.Net.Quic.dll` | 350 KB | [`16f6b9232becca34...`](https://www.virustotal.com/gui/file/16f6b9232becca3460e98f84d007b59f0d3db52372f7daae91ab315d95071241) |
| `sidecar/System.Net.Requests.dll` | 366 KB | [`7942b966861e88d5...`](https://www.virustotal.com/gui/file/7942b966861e88d53fcf6b9e2225ae9339211ecea137d642a770d2a7a6cd422f) |
| `sidecar/System.Net.Security.dll` | 650 KB | [`f799f93fbad3c47e...`](https://www.virustotal.com/gui/file/f799f93fbad3c47ea98133afc31011b68dd808813b4fb73208c6dda7bdd657b4) |
| `sidecar/System.Net.ServerSentEvents.dll` | 82 KB | [`6cc0f51450981688...`](https://www.virustotal.com/gui/file/6cc0f5145098168824cb6ccec1227e7f30ceb8f9756bfcb93f9faa89a97f5016) |
| `sidecar/System.Net.ServicePoint.dll` | 15 KB | [`61ea5c4619572e1c...`](https://www.virustotal.com/gui/file/61ea5c4619572e1cce028e75e4184901ecf2bd7aaf2f93e498b448a7fa3750ac) |
| `sidecar/System.Net.Sockets.dll` | 518 KB | [`53811a06f50f436d...`](https://www.virustotal.com/gui/file/53811a06f50f436db22d1279e5a209220ff363154e7c8981c8248d38f4c7798b) |
| `sidecar/System.Net.WebClient.dll` | 162 KB | [`7f4a639cf94a0856...`](https://www.virustotal.com/gui/file/7f4a639cf94a0856bc1496dea2e2a998d5784e505a197fcac1b5d07e140b26ad) |
| `sidecar/System.Net.WebHeaderCollection.dll` | 62 KB | [`36a83ef10e19143a...`](https://www.virustotal.com/gui/file/36a83ef10e19143a421839d2ae9634b22982e94feb7d0e0a6a602d057de093e6) |
| `sidecar/System.Net.WebProxy.dll` | 42 KB | [`1cd3e7330632c77d...`](https://www.virustotal.com/gui/file/1cd3e7330632c77de0eb0a30ced6a99b97bfe36418cafddcd0de525010a65219) |
| `sidecar/System.Net.WebSockets.Client.dll` | 102 KB | [`2ddca43e2f4f81c0...`](https://www.virustotal.com/gui/file/2ddca43e2f4f81c06afdf1fd0556a7533499d5c440310341bbc287060d3bacf3) |
| `sidecar/System.Net.WebSockets.dll` | 238 KB | [`2794e1d55e990713...`](https://www.virustotal.com/gui/file/2794e1d55e990713018d520a3f2c3cdc764db44816abcd7272f0f3a110b6d5d1) |
| `sidecar/System.Numerics.dll` | 15 KB | [`2cce5d7469a5e13b...`](https://www.virustotal.com/gui/file/2cce5d7469a5e13bb7b8380da457c75e406ae260faceb7d5fcfc7efbddd02e2f) |
| `sidecar/System.Numerics.Tensors.dll` | 506 KB | [`cd6878734a1fee6e...`](https://www.virustotal.com/gui/file/cd6878734a1fee6ecb50e3e151576cb7c7547bf2ddf5d480e95c01549e156c8c) |
| `sidecar/System.Numerics.Vectors.dll` | 15 KB | [`444d4cf2eb56a1e6...`](https://www.virustotal.com/gui/file/444d4cf2eb56a1e61ea5413813c9aab0d6a47749e620526907b4caa7db95edc6) |
| `sidecar/System.ObjectModel.dll` | 78 KB | [`12cb278af864552e...`](https://www.virustotal.com/gui/file/12cb278af864552e15af8c2ad2b0a29f2d4c571431b4d90a0c2e8ed9e57d90be) |
| `sidecar/System.Private.CoreLib.dll` | 15.29 MB | [`76a49f17e83f613b...`](https://www.virustotal.com/gui/file/76a49f17e83f613bc8a5b55fc98830b764e7838265aacbdbcc8f46707f80a3c9) |
| `sidecar/System.Private.DataContractSerialization.dll` | 1.97 MB | [`61c1bae125a218e4...`](https://www.virustotal.com/gui/file/61c1bae125a218e45e0bf517dc6633263280408c96c6d367d1839ee6a8bae936) |
| `sidecar/System.Private.Uri.dll` | 250 KB | [`27b93d6e754d7d2e...`](https://www.virustotal.com/gui/file/27b93d6e754d7d2e538333e3c1d520c64d7eb012dde1705e776177b273662b7d) |
| `sidecar/System.Private.Xml.dll` | 7.43 MB | [`d62389b5b3617ec4...`](https://www.virustotal.com/gui/file/d62389b5b3617ec44e31232440a525a2618d5056b4b9ed86abf51250edf7bca0) |
| `sidecar/System.Private.Xml.Linq.dll` | 382 KB | [`05462aa6f954c3c3...`](https://www.virustotal.com/gui/file/05462aa6f954c3c3c52c668416efcad04b4b2ebe37df247fe20ed5caa47e7a6e) |
| `sidecar/System.Reflection.DispatchProxy.dll` | 74 KB | [`85c4c9d47aded01f...`](https://www.virustotal.com/gui/file/85c4c9d47aded01fd0c01780b8152aadf1cc20ac97853764756734b872a0eda0) |
| `sidecar/System.Reflection.dll` | 16 KB | [`5e915aa614291026...`](https://www.virustotal.com/gui/file/5e915aa614291026130ba1b619294868a8d9b4d9dfff1aa5cb8b745bcc451756) |
| `sidecar/System.Reflection.Emit.dll` | 302 KB | [`d995259a96cba4d1...`](https://www.virustotal.com/gui/file/d995259a96cba4d1e13d8e65da0cc835af1961d9cf46340531d7d8ba3adbc6a3) |
| `sidecar/System.Reflection.Emit.ILGeneration.dll` | 15 KB | [`ab4bbc69ccddc720...`](https://www.virustotal.com/gui/file/ab4bbc69ccddc720351b4293890d4461740203e54df614b5605fd69d2c4c8148) |
| `sidecar/System.Reflection.Emit.Lightweight.dll` | 15 KB | [`99cd08aec7ed10f9...`](https://www.virustotal.com/gui/file/99cd08aec7ed10f906dd9560f194a1c6ef843b60c1e34cab0bd6815e13725c19) |
| `sidecar/System.Reflection.Extensions.dll` | 15 KB | [`dd4cf0a2f78784a5...`](https://www.virustotal.com/gui/file/dd4cf0a2f78784a58a0d9b937cd00c8d030940b79b9110c3420d388078b5869f) |
| `sidecar/System.Reflection.Metadata.dll` | 1.10 MB | [`5e745d60bc682671...`](https://www.virustotal.com/gui/file/5e745d60bc682671b9a0813f515eca84b61710e1f84136b69fc09ec8821f92b2) |
| `sidecar/System.Reflection.Primitives.dll` | 15 KB | [`d25735120c998ff3...`](https://www.virustotal.com/gui/file/d25735120c998ff3fbd5cd9a0fee8033bf6736e88dc4efe59c7789f44a905e6a) |
| `sidecar/System.Reflection.TypeExtensions.dll` | 42 KB | [`3438380d510ed092...`](https://www.virustotal.com/gui/file/3438380d510ed09241b34428690f752a1e08fbe392aefdb3c76ec3819af12156) |
| `sidecar/System.Resources.Reader.dll` | 15 KB | [`21e2a6135134183e...`](https://www.virustotal.com/gui/file/21e2a6135134183ef040aed2ff94de50efa63c1d2124aa0337ee49139d6c8299) |
| `sidecar/System.Resources.ResourceManager.dll` | 15 KB | [`dcda318334fbc517...`](https://www.virustotal.com/gui/file/dcda318334fbc517495c7fe6ec335cfe856a634112119c63b35a9e4ce9b9f667) |
| `sidecar/System.Resources.Writer.dll` | 50 KB | [`d3d5485df887aab8...`](https://www.virustotal.com/gui/file/d3d5485df887aab86de76c4d22d19641b6cbbd79754ca89c6805a6ce45baf60a) |
| `sidecar/System.Runtime.CompilerServices.Unsafe.dll` | 15 KB | [`50b76c3723b529d6...`](https://www.virustotal.com/gui/file/50b76c3723b529d6d6fde67b297409f875e20c7f7d45b8497be379c24207a322) |
| `sidecar/System.Runtime.CompilerServices.VisualC.dll` | 30 KB | [`95f383b9cc7811be...`](https://www.virustotal.com/gui/file/95f383b9cc7811be9b9aec45591fd2e0dbf6cd09406a29a3b3d8ed980f7d02b4) |
| `sidecar/System.Runtime.dll` | 44 KB | [`60d0795848d15c0e...`](https://www.virustotal.com/gui/file/60d0795848d15c0ee9c37d6b97dfef12eb48292ffda3e9054204b2c3a6a89b19) |
| `sidecar/System.Runtime.Extensions.dll` | 17 KB | [`a474fd258722ffb4...`](https://www.virustotal.com/gui/file/a474fd258722ffb4af11ecd6e5765a27713307faa11dbf804414814a2099bb5a) |
| `sidecar/System.Runtime.Handles.dll` | 15 KB | [`1f6aaa3ca188434c...`](https://www.virustotal.com/gui/file/1f6aaa3ca188434c80ff6fad600915e0d2f3ada0746ff5d0f0ecf13ab398ac41) |
| `sidecar/System.Runtime.InteropServices.dll` | 110 KB | [`627a86a918f6669f...`](https://www.virustotal.com/gui/file/627a86a918f6669f94cf8db6ce064a4b2207b852ba6a4fecaccad7bb07859b4f) |
| `sidecar/System.Runtime.InteropServices.JavaScript.dll` | 50 KB | [`03dfff8c7082a648...`](https://www.virustotal.com/gui/file/03dfff8c7082a64866feae2d0822da69708900d7635bb48b5b73881c8292cad0) |
| `sidecar/System.Runtime.InteropServices.RuntimeInformation.dll` | 15 KB | [`cc959b69928f2842...`](https://www.virustotal.com/gui/file/cc959b69928f28420d31360ee8bf793c6f3e4d5809752cf1d386efe01dc49aaa) |
| `sidecar/System.Runtime.Intrinsics.dll` | 17 KB | [`a24e7018598b352d...`](https://www.virustotal.com/gui/file/a24e7018598b352d1ade1bc452b2c65a31902b56ce1fc1fccd8b26e2677943c8) |
| `sidecar/System.Runtime.Loader.dll` | 15 KB | [`404791be0d21a5ee...`](https://www.virustotal.com/gui/file/404791be0d21a5ee0119ec08416a5ae18f3bbfbbc9f4d6a6ad7c3a9c667aaa25) |
| `sidecar/System.Runtime.Numerics.dll` | 346 KB | [`8c715e55c80a4ab3...`](https://www.virustotal.com/gui/file/8c715e55c80a4ab31d086b5e51261d691b52b867b1c1f5d99c344ea4faa27f4b) |
| `sidecar/System.Runtime.Serialization.dll` | 17 KB | [`1f4c9a42ec5b5d4d...`](https://www.virustotal.com/gui/file/1f4c9a42ec5b5d4da23a82fc1952e01cf395e99cfabc8a7d0f5a4b304e24915a) |
| `sidecar/System.Runtime.Serialization.Formatters.dll` | 122 KB | [`a971cba21d6064c5...`](https://www.virustotal.com/gui/file/a971cba21d6064c58b38c10cb04092dd4b8ab2919a9f5544e94082154f5604ce) |
| `sidecar/System.Runtime.Serialization.Json.dll` | 15 KB | [`473344505e448744...`](https://www.virustotal.com/gui/file/473344505e44874494bd6a2964da9860c224dfab884a10e43992df868b44093f) |
| `sidecar/System.Runtime.Serialization.Primitives.dll` | 38 KB | [`6e19e864162258ea...`](https://www.virustotal.com/gui/file/6e19e864162258ea14a5d15a0e5b6b08368aabd087d0696032338dc6bd48d71b) |
| `sidecar/System.Runtime.Serialization.Xml.dll` | 16 KB | [`a3d3d53a385bbe63...`](https://www.virustotal.com/gui/file/a3d3d53a385bbe636dff506c09d008fe0d7c53d9e1a2ad431a2b6ddd5e0200ae) |
| `sidecar/System.Security.AccessControl.dll` | 222 KB | [`0ce0621ed9cf8fde...`](https://www.virustotal.com/gui/file/0ce0621ed9cf8fde8aa78be726f7e4ba7b9e321c86ff1faa282a8e8ca1a62e86) |
| `sidecar/System.Security.Claims.dll` | 98 KB | [`29f395b2a9bd6de2...`](https://www.virustotal.com/gui/file/29f395b2a9bd6de2a80c38c02354b74a7cca434da5125feb4a649787981b8fc6) |
| `sidecar/System.Security.Cryptography.Algorithms.dll` | 17 KB | [`06a50cccb59d6485...`](https://www.virustotal.com/gui/file/06a50cccb59d64852893af1794c71f6008b4fb83d7ab980df4459c202f42d726) |
| `sidecar/System.Security.Cryptography.Cng.dll` | 16 KB | [`f09ca990c590839d...`](https://www.virustotal.com/gui/file/f09ca990c590839dd572d52daf96eb10472ace1c6a18c9c1b56bf88c5a78953b) |
| `sidecar/System.Security.Cryptography.Csp.dll` | 16 KB | [`8f69002eb9a370c9...`](https://www.virustotal.com/gui/file/8f69002eb9a370c998d243f7c1427f40d40d7665c71d30f9767919b26088ef4e) |
| `sidecar/System.Security.Cryptography.dll` | 2.43 MB | [`637104ff4a89fa34...`](https://www.virustotal.com/gui/file/637104ff4a89fa341aad178fa4c6c85f247889d8d544f62411cc3febed9679b0) |
| `sidecar/System.Security.Cryptography.Encoding.dll` | 15 KB | [`82529d4194905c19...`](https://www.virustotal.com/gui/file/82529d4194905c197049fb657402e50566e5566a6c62b68cfbce1b5409f99d63) |
| `sidecar/System.Security.Cryptography.OpenSsl.dll` | 15 KB | [`cfb536a8f944cf0d...`](https://www.virustotal.com/gui/file/cfb536a8f944cf0dea22a0f19ed618fe610ce56ad440fde85d606e5a148683ca) |
| `sidecar/System.Security.Cryptography.Primitives.dll` | 15 KB | [`a371b402dadb1f19...`](https://www.virustotal.com/gui/file/a371b402dadb1f19468862d1deba44b5d08e0fa633904742ec5c8c1922ed6081) |
| `sidecar/System.Security.Cryptography.X509Certificates.dll` | 16 KB | [`5e0e0595a937695c...`](https://www.virustotal.com/gui/file/5e0e0595a937695c7cfd3099245816177e7c2b1233742274f28c93ff8d3c7bcd) |
| `sidecar/System.Security.dll` | 18 KB | [`0a9e88553523ebef...`](https://www.virustotal.com/gui/file/0a9e88553523ebefa42525def2a92577c9cc5c87b20bc6c3bcc386948388ba87) |
| `sidecar/System.Security.Principal.dll` | 15 KB | [`45a74a4b6175b9d2...`](https://www.virustotal.com/gui/file/45a74a4b6175b9d228969592a3175a8811aba9a74c94676aa7e46c0cdc125313) |
| `sidecar/System.Security.Principal.Windows.dll` | 174 KB | [`b04ee5bd519137cc...`](https://www.virustotal.com/gui/file/b04ee5bd519137cc929b355221af2d9649f149aca7808768a0cb99e423ff8668) |
| `sidecar/System.Security.SecureString.dll` | 15 KB | [`3d548514379830c1...`](https://www.virustotal.com/gui/file/3d548514379830c108543f276751874cb1a6d930f9b54f80953e72e90dca70a4) |
| `sidecar/System.ServiceModel.Web.dll` | 16 KB | [`2c57d8b0f8d2a910...`](https://www.virustotal.com/gui/file/2c57d8b0f8d2a91088d1a074fb40ca8c1479a9d47071e5f99de5c11638d2edf7) |
| `sidecar/System.ServiceProcess.dll` | 15 KB | [`5cdd7ad9adb627d4...`](https://www.virustotal.com/gui/file/5cdd7ad9adb627d475931fe9f93d557fcd826ef393254b8a11a8c3c04b643453) |
| `sidecar/System.Text.Encoding.CodePages.dll` | 842 KB | [`e1ef93444fa374ef...`](https://www.virustotal.com/gui/file/e1ef93444fa374ef5267d2a49751d60e6ee832da88988d129977adfa881f462e) |
| `sidecar/System.Text.Encoding.dll` | 15 KB | [`96a719292c975223...`](https://www.virustotal.com/gui/file/96a719292c97522390027535db13529173e5a3c728209c9d90dbd1b50fa71cb3) |
| `sidecar/System.Text.Encoding.Extensions.dll` | 15 KB | [`81229e9c66c02dbd...`](https://www.virustotal.com/gui/file/81229e9c66c02dbd1cc4de1cd58f6e6069a652e55e4375c8b18d987145479a04) |
| `sidecar/System.Text.Encodings.Web.dll` | 122 KB | [`775eab42196bf946...`](https://www.virustotal.com/gui/file/775eab42196bf9466a69e9fc86e47b8da620b7652aa4b8a62245449131ce885d) |
| `sidecar/System.Text.Json.dll` | 1.80 MB | [`6945c5fc52f811e3...`](https://www.virustotal.com/gui/file/6945c5fc52f811e32b13597b0d2a14d365227245e21bf7daebc6986f729b25fa) |
| `sidecar/System.Text.RegularExpressions.dll` | 1,010 KB | [`b1f6aec1ed431cf5...`](https://www.virustotal.com/gui/file/b1f6aec1ed431cf5956d8bc870fb0d47c43455438c5c0110433fd125183f190d) |
| `sidecar/System.Threading.AccessControl.dll` | 74 KB | [`14e0ec8285100d4a...`](https://www.virustotal.com/gui/file/14e0ec8285100d4a3259a899d9ffa5269797e8ae2e3c9e119783ece06ebc62c1) |
| `sidecar/System.Threading.Channels.dll` | 154 KB | [`37349d0028f2dde5...`](https://www.virustotal.com/gui/file/37349d0028f2dde5da668cf7e8bfb56fa7ab583997a44e41e4da0b0fd00f8516) |
| `sidecar/System.Threading.dll` | 82 KB | [`96ef32e315217f24...`](https://www.virustotal.com/gui/file/96ef32e315217f24b6b701fc182cac464966cad1da94dd25a3609fe2d9f5a6ca) |
| `sidecar/System.Threading.Overlapped.dll` | 15 KB | [`d5e2b62198f2a9e1...`](https://www.virustotal.com/gui/file/d5e2b62198f2a9e1845a8bdd874f3f2e232eeec7a178ad1ca2313a5d9f183ab2) |
| `sidecar/System.Threading.Tasks.Dataflow.dll` | 462 KB | [`cfcff09f4aee061e...`](https://www.virustotal.com/gui/file/cfcff09f4aee061e3eca6792908051f272c1b8b9d3d81fe1858ae047648c4ebc) |
| `sidecar/System.Threading.Tasks.dll` | 16 KB | [`2830ac695731137a...`](https://www.virustotal.com/gui/file/2830ac695731137a529b7620482499eae9dc88730aa78b2dbbbe35e9ea6655d9) |
| `sidecar/System.Threading.Tasks.Extensions.dll` | 15 KB | [`5f042ed8fcd4fcfb...`](https://www.virustotal.com/gui/file/5f042ed8fcd4fcfbab2572e5f54b0b74f959d1f52404365450f763ef5400180b) |
| `sidecar/System.Threading.Tasks.Parallel.dll` | 130 KB | [`ec95052ee31ac8bc...`](https://www.virustotal.com/gui/file/ec95052ee31ac8bc4a08241939c3d3177ac671470caf898b8218245523ce4b21) |
| `sidecar/System.Threading.Thread.dll` | 15 KB | [`40cd4539f24de040...`](https://www.virustotal.com/gui/file/40cd4539f24de04085f5b1f6c12680e0703368a13d2ef96fe8c20c7602cce2c2) |
| `sidecar/System.Threading.ThreadPool.dll` | 15 KB | [`8d12767f122b5da3...`](https://www.virustotal.com/gui/file/8d12767f122b5da33597ac423d19c693fe6d8615d7e1aeaba9e49e2a6bcf16c5) |
| `sidecar/System.Threading.Timer.dll` | 15 KB | [`37eb792b476add15...`](https://www.virustotal.com/gui/file/37eb792b476add157a6c9837ccb81d4b7ff1d2cbcecf055ec8610b57053206c6) |
| `sidecar/System.Transactions.dll` | 16 KB | [`a330fd4b6173de21...`](https://www.virustotal.com/gui/file/a330fd4b6173de21caf33c0ac79d329097791e688ab58926d2dc68341e853d17) |
| `sidecar/System.Transactions.Local.dll` | 626 KB | [`f6fca23ae3902c9e...`](https://www.virustotal.com/gui/file/f6fca23ae3902c9e8f3b1951d26ea5d831bdf80b217d2e5a9f56deb5b1d90229) |
| `sidecar/System.ValueTuple.dll` | 15 KB | [`069f72a5d77ef21f...`](https://www.virustotal.com/gui/file/069f72a5d77ef21ff8d60c5b63c7e894999ebece4c8cfafc697aaa3bdc5605ae) |
| `sidecar/System.Web.dll` | 15 KB | [`934c6ef3f2bc8959...`](https://www.virustotal.com/gui/file/934c6ef3f2bc8959aba62ced6d1cef069d565210e65bf59b5980e7e39571528e) |
| `sidecar/System.Web.HttpUtility.dll` | 62 KB | [`1f8bf172cd56387c...`](https://www.virustotal.com/gui/file/1f8bf172cd56387c0c348bc65cf6a31548324da7ff7347d5190d84e9ff82a475) |
| `sidecar/System.Windows.dll` | 15 KB | [`ffd590ee80328cf9...`](https://www.virustotal.com/gui/file/ffd590ee80328cf902b4d66a8db1b2d34b7ad5ba8fdb93db51c4dc0c5829439a) |
| `sidecar/System.Xml.dll` | 23 KB | [`60eebcb2939feb3c...`](https://www.virustotal.com/gui/file/60eebcb2939feb3c23686c067a9f49a93020488c2594357670aac4e568d380b0) |
| `sidecar/System.Xml.Linq.dll` | 15 KB | [`519999617d619d88...`](https://www.virustotal.com/gui/file/519999617d619d88f5489021ba690277a2648bc6b344e1649be679f0e51e6ccb) |
| `sidecar/System.Xml.ReaderWriter.dll` | 21 KB | [`cdd1a8424361a334...`](https://www.virustotal.com/gui/file/cdd1a8424361a33498e2a69051a7330ed356927f675b917422861138c99f6406) |
| `sidecar/System.Xml.Serialization.dll` | 16 KB | [`b2cf0f39e9e2d086...`](https://www.virustotal.com/gui/file/b2cf0f39e9e2d0867a899a7fd4a889ee6ad3cc6ed48a7079b28502d7c808a73e) |
| `sidecar/System.Xml.XDocument.dll` | 15 KB | [`bc290c6ecf7ff713...`](https://www.virustotal.com/gui/file/bc290c6ecf7ff713a492b48f9d507cbfe85a50ccd00e187e68f422d75804efec) |
| `sidecar/System.Xml.XmlDocument.dll` | 15 KB | [`4d07558fb3573e4a...`](https://www.virustotal.com/gui/file/4d07558fb3573e4a0d91b928361060d1fe7162b9017e17e759ad77c8fb6986ca) |
| `sidecar/System.Xml.XmlSerializer.dll` | 17 KB | [`2c7cfc2a8ec748d9...`](https://www.virustotal.com/gui/file/2c7cfc2a8ec748d920753b7328c681f56a65608d029f36698b0af79945c0c253) |
| `sidecar/System.Xml.XPath.dll` | 15 KB | [`2f25f882a1816e7a...`](https://www.virustotal.com/gui/file/2f25f882a1816e7a6bb0445562a4b7cc1653b25c0697b184ab6808b22daa49bb) |
| `sidecar/System.Xml.XPath.XDocument.dll` | 30 KB | [`dd5ee3b63637b62a...`](https://www.virustotal.com/gui/file/dd5ee3b63637b62a7808f4e8b8e6299258c0b043d8d103ff1f0b7d31230b7dcb) |
| `sidecar/WindowsBase.dll` | 16 KB | [`40919153c0d41db6...`](https://www.virustotal.com/gui/file/40919153c0d41db63182208f07ceb2cdd7a5eaa1231bfae318f64fad929d6646) |

</details>

## Nothing else ships

The archive holds 240 files. The tables above cover 231 of them: every executable, every native library,
and both model files. The remaining 9 carry no code and are listed here so the accounting is complete.

| File | Size | SHA256 |
|---|---|---|
| `characters/linus.json` | 1 KB | `ba4cbcc6b9a50b57f7b0cbb7ca2b65de83ecf0d77a6fdca87b0ddb297cdedf87` |
| `config.json` | 491 B | `a5ed8488a52347610ce59eb0af74e4fa9286da0b036021da0e56e6d2ed3e0a2b` |
| `LICENSE` | 11 KB | `3ddf9be5c28fe27dad143a5dc76eea25222ad1dd68934a047064e56ed2fa40c5` |
| `LICENSE-LFM.txt` | 11 KB | `622215e455bf5452a7edd091324c49195c119ed8b901238fbf331a0675dd9dc5` |
| `manifest.json` | 457 B | `9f0fc8dc3e0acc313101a31cff82e0b9b03dc32b8360da49c80b37c714cee1fb` |
| `NOTICE` | 2 KB | `7118955af7c61dd25c73878338eb767180443e12398492f921fb85a099dd2ac6` |
| `README.txt` | 2 KB | `cb45c6ad5aa56eb1e900296a45205f19aff0847171ad7d45a136cc813ea050f8` |
| `sidecar/ChattyValley.Sidecar.deps.json` | 36 KB | `d3feb793739e8e374d06bb986efb9c30ad6407ec975bcc3e1cd65961f4332c56` |
| `sidecar/ChattyValley.Sidecar.runtimeconfig.json` | 374 B | `555c1ff1b1fda2619a3573046b9eb34d2d781ac96b60e29626016e232389d027` |
