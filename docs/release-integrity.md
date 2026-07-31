# Release integrity

Generated from `ChattyValley-0.2.0.zip` by `scripts/generate-release-manifest.ps1`. Every hash below comes from that
artifact's own bytes, so this document is only true for that build. It is regenerated with each release.

- **Archive:** `ChattyValley-0.2.0.zip`, 744.35 MB (780,505,290 bytes)
- **SHA256:** `cd1453c70f0e4aa88008f69ceea5115774bf77a1be49ffa55f6983aef658ba2b`

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
| `sidecar/ChattyValley.Sidecar.exe` | 159 KB | [`b8f41bef51bc5e26...`](https://www.virustotal.com/gui/file/b8f41bef51bc5e26e10d6c697d16f82f538e0ccb26c82a3c34ad7872e91b28ac) |

## llama.cpp inference, native

20 files, 19 unique binaries. Four builds ship per library, one per CPU instruction set, and your
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

## This project's own code, managed

Small because they are just the mod logic. No antivirus engine holds signatures for a single developer's
assemblies, so a clean result here proves less than reading the source, which is the point of this repo.

| File | Size | SHA256 |
|---|---|---|
| `ChattyValley.Core.dll` | 20 KB | [`83185749fc06d66e...`](https://www.virustotal.com/gui/file/83185749fc06d66e3e303be12bcdf8b09c8fe9b7f4a21943415b9ac3e610f4a1) |
| `ChattyValley.Mod.dll` | 51 KB | [`41501896c7832b6f...`](https://www.virustotal.com/gui/file/41501896c7832b6f6bc2ab28fb5ce46020caea5d352e27b6607db8f518a6e131) |
| `sidecar/ChattyValley.Core.dll` | 20 KB | [`83185749fc06d66e...`](https://www.virustotal.com/gui/file/83185749fc06d66e3e303be12bcdf8b09c8fe9b7f4a21943415b9ac3e610f4a1) |
| `sidecar/ChattyValley.Runtime.dll` | 14 KB | [`312f420f3b82f42d...`](https://www.virustotal.com/gui/file/312f420f3b82f42d0360299ca769ce259b6fd49b182deabeb2d7163c8d6d25da) |
| `sidecar/ChattyValley.Sidecar.dll` | 16 KB | [`33b92cf53bd31f7a...`](https://www.virustotal.com/gui/file/33b92cf53bd31f7ac1b985021ce822b61f3de5c7fddcecf462ad1a0996223067) |

## Model weights

Data, not code. Nothing executes these; `llama.cpp` reads them as tensors. Listed for completeness and so
you can confirm the download is intact. The base model exceeds VirusTotal's upload limit, so these carry
hashes rather than scan links.

| File | Size | SHA256 |
|---|---|---|
| `assets/LFM2.5-1.2B-Instruct-Q4_K_M.gguf` | 697.04 MB | `b1b3de114215d9507409a662a501a631095a479a419584e8a2ded6304b19b4f5` |
| `assets/linus-12b-v8dpo2-lora-f16.gguf` | 21.20 MB | `d6371a5c2ce12a9c752989f52ffa6956475bbe8ac54c7800112d0d525eeb7610` |

## Microsoft .NET runtime

197 files, 79.99 MB. The sidecar ships self-contained so players need no .NET install. These are Microsoft's,
unmodified and Microsoft-signed; scanning them tells you about Microsoft rather than about this mod. Listed
in full anyway, because "every executable file" should mean every one.

<details><summary>Show all 197 runtime files</summary>

| File | Size | SHA256 |
|---|---|---|
| `sidecar/clretwrc.dll` | 311 KB | [`75b0b385413ed782...`](https://www.virustotal.com/gui/file/75b0b385413ed78263fffa8963ecb928af8624c46b1876807e04586275cbc861) |
| `sidecar/clrgc.dll` | 658 KB | [`3fd517761cd28789...`](https://www.virustotal.com/gui/file/3fd517761cd287892a576088ddd5cdbf76f8463573df085cf84354ac033e35e2) |
| `sidecar/clrgcexp.dll` | 693 KB | [`14cbf8329ea868eb...`](https://www.virustotal.com/gui/file/14cbf8329ea868eb382467174dabf840271e138889752654692d0fb1f2d3cb55) |
| `sidecar/clrjit.dll` | 2.10 MB | [`b99554ea472b27b4...`](https://www.virustotal.com/gui/file/b99554ea472b27b4a93ec232e793d81e444051dfdbaf0552f8262c58723a64f2) |
| `sidecar/CommunityToolkit.HighPerformance.dll` | 150 KB | [`196607c6d4d704b5...`](https://www.virustotal.com/gui/file/196607c6d4d704b5ee95c80f8e3cd799595b7d8a08108701894c211b138d2839) |
| `sidecar/coreclr.dll` | 4.52 MB | [`ae97f4a37d0dbd78...`](https://www.virustotal.com/gui/file/ae97f4a37d0dbd781639c69bf0960f48b53b8a81ba71076460d8f80388334ff8) |
| `sidecar/createdump.exe` | 70 KB | [`642774df8d4d375e...`](https://www.virustotal.com/gui/file/642774df8d4d375e6b5303362739d839947fcbb5b6eb90024e2937ff58cd89a5) |
| `sidecar/hostfxr.dll` | 371 KB | [`1b12d4a43921951b...`](https://www.virustotal.com/gui/file/1b12d4a43921951b03927da4cf68dbb59069da5d7da08a200dfb8c5a8888f758) |
| `sidecar/hostpolicy.dll` | 370 KB | [`6bd7f6420ced2454...`](https://www.virustotal.com/gui/file/6bd7f6420ced2454a407add25c424de0d76e66c727a95b45d047342d39a72e87) |
| `sidecar/LLamaSharp.dll` | 283 KB | [`559fd41e6a9757cc...`](https://www.virustotal.com/gui/file/559fd41e6a9757ccd7aaec61255ecde33bc0294894510e10b94d095e02ad7c10) |
| `sidecar/Microsoft.Bcl.AsyncInterfaces.dll` | 20 KB | [`e925fe423d376b8c...`](https://www.virustotal.com/gui/file/e925fe423d376b8c795c17ef57ef61a749f3bd34f044e72b2243103feadac1ec) |
| `sidecar/Microsoft.Bcl.Memory.dll` | 16 KB | [`d844a2e31b7a0abc...`](https://www.virustotal.com/gui/file/d844a2e31b7a0abc10911608e0a819523aa055774a5b46e8e12de14cd5a44b32) |
| `sidecar/Microsoft.CSharp.dll` | 958 KB | [`d004c745f1f0c003...`](https://www.virustotal.com/gui/file/d004c745f1f0c00373fa06817f0bda682afea3b0bf9f791cfd9103dfbb2cdc21) |
| `sidecar/Microsoft.DiaSymReader.Native.amd64.dll` | 2.20 MB | [`7f8dcf37d9d66eae...`](https://www.virustotal.com/gui/file/7f8dcf37d9d66eae14c48a79fa2fcd447bd0f38a21be0203a9c4a89398aacf28) |
| `sidecar/Microsoft.Extensions.AI.Abstractions.dll` | 654 KB | [`688fea67d5323883...`](https://www.virustotal.com/gui/file/688fea67d5323883bd10086a83a6aac01f791830dcebbefb03515c9002a9d8f3) |
| `sidecar/Microsoft.Extensions.DependencyInjection.Abstractions.dll` | 65 KB | [`842fc19802a761e4...`](https://www.virustotal.com/gui/file/842fc19802a761e49c65b0e90b2c68bc6868b989145da5ac6ca30b766e2658f2) |
| `sidecar/Microsoft.Extensions.Logging.Abstractions.dll` | 66 KB | [`03e916b9c5f91717...`](https://www.virustotal.com/gui/file/03e916b9c5f91717b67c790325d4e5dd2debe81d8e403928806ba1d430ea7147) |
| `sidecar/Microsoft.VisualBasic.Core.dll` | 1.14 MB | [`e5f3c2dbcb245254...`](https://www.virustotal.com/gui/file/e5f3c2dbcb2452548cbc916da03e7666d10e701b246b1c3f7c0f867441e03f35) |
| `sidecar/Microsoft.VisualBasic.dll` | 17 KB | [`e42fb94db00c709e...`](https://www.virustotal.com/gui/file/e42fb94db00c709ef51392984e6a468dbfaf61804373ce19f2dd59adbce41b1c) |
| `sidecar/Microsoft.Win32.Primitives.dll` | 16 KB | [`9e70ff30ecb66fde...`](https://www.virustotal.com/gui/file/9e70ff30ecb66fde0fb0a3e1e2ab0381a56a5d11007f73fe38e8a6f84fd86f4c) |
| `sidecar/Microsoft.Win32.Registry.dll` | 114 KB | [`f9957093a5206b0c...`](https://www.virustotal.com/gui/file/f9957093a5206b0c2d863703e4c23e0c38c8f9317f0feec32c52138ed25704d6) |
| `sidecar/mscordaccore.dll` | 1.29 MB | [`ea9e08c32c81aa02...`](https://www.virustotal.com/gui/file/ea9e08c32c81aa02bbcc07b9372dcb7ae403da116eb445c0319eb6340f6a5e83) |
| `sidecar/mscordaccore_amd64_amd64_10.0.225.61305.dll` | 1.29 MB | [`ea9e08c32c81aa02...`](https://www.virustotal.com/gui/file/ea9e08c32c81aa02bbcc07b9372dcb7ae403da116eb445c0319eb6340f6a5e83) |
| `sidecar/mscordbi.dll` | 1.18 MB | [`9ed47fd693951e76...`](https://www.virustotal.com/gui/file/9ed47fd693951e766b6ac6097ff82aa9f6011e4ee7f6ba23bb68f622f87cc4c8) |
| `sidecar/mscorlib.dll` | 59 KB | [`684e2782cdb335bc...`](https://www.virustotal.com/gui/file/684e2782cdb335bc8489bd6a72e01d4ea622a155561315aa63b8c06629bbcc57) |
| `sidecar/mscorrc.dll` | 131 KB | [`ffad4ed1f225571d...`](https://www.virustotal.com/gui/file/ffad4ed1f225571ddb5a767f35c0349409efff011c3624c9912be7ea7580a44e) |
| `sidecar/msquic.dll` | 512 KB | [`6246f5fe726fbf61...`](https://www.virustotal.com/gui/file/6246f5fe726fbf612ca621961fd90a440425d64e9f4e51b7bb7adb80651032fa) |
| `sidecar/netstandard.dll` | 99 KB | [`850e5a52885a5164...`](https://www.virustotal.com/gui/file/850e5a52885a51649626eccebf78bc5c461a7b23b36e10997f624bb1d0aa398d) |
| `sidecar/System.AppContext.dll` | 15 KB | [`557d6e9ae495a068...`](https://www.virustotal.com/gui/file/557d6e9ae495a068003971eae85d242ac6b50de18d8e3a7877aca11fc27d3c07) |
| `sidecar/System.Buffers.dll` | 15 KB | [`b3fd4be3951d896f...`](https://www.virustotal.com/gui/file/b3fd4be3951d896fd13e22b51c2b172513c5487430a370d8c9ae3ea666f10056) |
| `sidecar/System.Collections.Concurrent.dll` | 290 KB | [`bb7f6b4cd731ea62...`](https://www.virustotal.com/gui/file/bb7f6b4cd731ea6228c912c8dde615da5405ae7ea8b1537d6c2ba6f63f3db90e) |
| `sidecar/System.Collections.dll` | 290 KB | [`607bbdb52a275eae...`](https://www.virustotal.com/gui/file/607bbdb52a275eae0311449c8423e0f7175d9d8c42e6a6e4c9d987e68740f2e0) |
| `sidecar/System.Collections.Immutable.dll` | 858 KB | [`96fce00abcc592ee...`](https://www.virustotal.com/gui/file/96fce00abcc592eeed599c72fcd524c444bb76e88c051b39c49fb3f0b7304ef7) |
| `sidecar/System.Collections.NonGeneric.dll` | 102 KB | [`73366c7058762b46...`](https://www.virustotal.com/gui/file/73366c7058762b467732f9932a2d83f2ecfc3f1f755dc91e51d747bbc5cf163d) |
| `sidecar/System.Collections.Specialized.dll` | 102 KB | [`1574f7db145d3639...`](https://www.virustotal.com/gui/file/1574f7db145d363905accdbd535feac581910a741f2d90978dccba1b79b0b11f) |
| `sidecar/System.ComponentModel.Annotations.dll` | 194 KB | [`975682305607f50c...`](https://www.virustotal.com/gui/file/975682305607f50cbe2abe6ef042cc3ad277ef84da70c68b129e9bef17625a1b) |
| `sidecar/System.ComponentModel.DataAnnotations.dll` | 17 KB | [`bf6c0992324b1654...`](https://www.virustotal.com/gui/file/bf6c0992324b1654678c17aaf753b1951f2bdd09a6832215bff5fe2d1bdf7773) |
| `sidecar/System.ComponentModel.dll` | 30 KB | [`8cabff4863dd55c6...`](https://www.virustotal.com/gui/file/8cabff4863dd55c6110ead70b38145ea5588e9674cc538e10d6aaf5494ef931a) |
| `sidecar/System.ComponentModel.EventBasedAsync.dll` | 46 KB | [`f96b9ec4113cfa06...`](https://www.virustotal.com/gui/file/f96b9ec4113cfa067141ba1021162c192634d70e216ef67116c64c70316e36b5) |
| `sidecar/System.ComponentModel.Primitives.dll` | 78 KB | [`a8dc1d2813df0556...`](https://www.virustotal.com/gui/file/a8dc1d2813df0556a187c4860c6909d851b1a938498f9f50d2d8ce2709b73797) |
| `sidecar/System.ComponentModel.TypeConverter.dll` | 750 KB | [`18a9508bae22acd1...`](https://www.virustotal.com/gui/file/18a9508bae22acd1ca80c657be13f7532cc3950352082ae9dc5d7da37eeaf7bb) |
| `sidecar/System.Configuration.dll` | 19 KB | [`ff8bfff364dfa41e...`](https://www.virustotal.com/gui/file/ff8bfff364dfa41e81b5251dc04d03fc6022b238926df308e556878dac9d9790) |
| `sidecar/System.Console.dll` | 166 KB | [`ccd66d99b980bd58...`](https://www.virustotal.com/gui/file/ccd66d99b980bd58de311528b071b9bcf64b348e286b28bbdb0e717c6a5565c6) |
| `sidecar/System.Core.dll` | 23 KB | [`7b50ee5bb3f97a54...`](https://www.virustotal.com/gui/file/7b50ee5bb3f97a5461b58edc45d1a0f8bd3abcf1624cdca72feaaef9d78c3fb3) |
| `sidecar/System.Data.Common.dll` | 2.64 MB | [`99d35f7555f599f8...`](https://www.virustotal.com/gui/file/99d35f7555f599f8541b5a9fa8d7e818d3fe103247b80890469e4288ef8374fb) |
| `sidecar/System.Data.DataSetExtensions.dll` | 16 KB | [`20b9eb4ed6399933...`](https://www.virustotal.com/gui/file/20b9eb4ed6399933dd51dfc305d17a805df4f5ce2502bb72ff39a126b8ee92da) |
| `sidecar/System.Data.dll` | 25 KB | [`44e8a3ed24381797...`](https://www.virustotal.com/gui/file/44e8a3ed24381797c83e6f3ab20eaa012ddc80fbddf82cc30ebe2673ac6597a6) |
| `sidecar/System.Diagnostics.Contracts.dll` | 16 KB | [`45dc32088cbb170c...`](https://www.virustotal.com/gui/file/45dc32088cbb170c3175f83757823b2b7d5ad7c64cd6b5da2738fc15fec7f181) |
| `sidecar/System.Diagnostics.Debug.dll` | 16 KB | [`3af918444c56fac5...`](https://www.virustotal.com/gui/file/3af918444c56fac50908c565a4d72407e4faac71f3f90630cde9487a298b9ccf) |
| `sidecar/System.Diagnostics.DiagnosticSource.dll` | 490 KB | [`b599497c6f52bdf4...`](https://www.virustotal.com/gui/file/b599497c6f52bdf497e3e5557e45263d33fa9b06226b6c0b70c98a0675cf40f6) |
| `sidecar/System.Diagnostics.FileVersionInfo.dll` | 46 KB | [`654f4e8d35ca9e37...`](https://www.virustotal.com/gui/file/654f4e8d35ca9e376eb4b105fe65acdb20a75c7d406fc222121cf9e39dae5510) |
| `sidecar/System.Diagnostics.Process.dll` | 322 KB | [`934782a25d2368e2...`](https://www.virustotal.com/gui/file/934782a25d2368e27807e28b549175bcf8dbc61c3598b5eba3a53143ee76124a) |
| `sidecar/System.Diagnostics.StackTrace.dll` | 42 KB | [`fa4d3047d72e1f12...`](https://www.virustotal.com/gui/file/fa4d3047d72e1f1266837f4f5d701a67e3b22322e88891a6f41ce25d70a8138f) |
| `sidecar/System.Diagnostics.TextWriterTraceListener.dll` | 66 KB | [`dbce5ba58bf1207e...`](https://www.virustotal.com/gui/file/dbce5ba58bf1207e961f076bd7263dfed24413975c50dc24884443e433a69565) |
| `sidecar/System.Diagnostics.Tools.dll` | 15 KB | [`70edb8f72a513a2a...`](https://www.virustotal.com/gui/file/70edb8f72a513a2a00d7987f53ee1c6121dc3b42e202a27b7e93bb61db025d17) |
| `sidecar/System.Diagnostics.TraceSource.dll` | 138 KB | [`cbb7546f36b7d7f9...`](https://www.virustotal.com/gui/file/cbb7546f36b7d7f936128c8f35df2d0492541097fe2f98dddc540007e5350ddc) |
| `sidecar/System.Diagnostics.Tracing.dll` | 16 KB | [`3a1bc6f29accafb1...`](https://www.virustotal.com/gui/file/3a1bc6f29accafb1c11f9253e71e875ac0b98bcfdb11cd477784dedc1a1941a7) |
| `sidecar/System.dll` | 50 KB | [`79b7e1d5ca82ea03...`](https://www.virustotal.com/gui/file/79b7e1d5ca82ea0326d44e3520bdfdbac605830f6f6e55aba3cc898df69a6500) |
| `sidecar/System.Drawing.dll` | 20 KB | [`ad3692e7e62a33ff...`](https://www.virustotal.com/gui/file/ad3692e7e62a33ffbbf1fbbba9770aa9bb038fa9792a3fd64b74032a01ecc065) |
| `sidecar/System.Drawing.Primitives.dll` | 130 KB | [`e74fb730c60f4d9b...`](https://www.virustotal.com/gui/file/e74fb730c60f4d9ba0dcce6584e048eddd66f3530604738dacf9cb1ba958bdf1) |
| `sidecar/System.Dynamic.Runtime.dll` | 16 KB | [`edd4d875ad0e6c4c...`](https://www.virustotal.com/gui/file/edd4d875ad0e6c4c78e83e2d04d532fb3aeb46ed10fd2101a453c6d4f48f1120) |
| `sidecar/System.Formats.Asn1.dll` | 234 KB | [`0f29b46c6714c7f4...`](https://www.virustotal.com/gui/file/0f29b46c6714c7f46291e8931cdbbf4d2f850329b14d860a91a74bac4195444f) |
| `sidecar/System.Formats.Tar.dll` | 262 KB | [`803586dc3911b16f...`](https://www.virustotal.com/gui/file/803586dc3911b16f4d346d6eaece17bda4db5b7a95b72d4bb94d99da341e8f7b) |
| `sidecar/System.Globalization.Calendars.dll` | 16 KB | [`0e081da984f85f5d...`](https://www.virustotal.com/gui/file/0e081da984f85f5d14481612b8f9769417936096dc616959a4faea2da7659f5d) |
| `sidecar/System.Globalization.dll` | 16 KB | [`c5111833fd153fa2...`](https://www.virustotal.com/gui/file/c5111833fd153fa252d59c9c7b9bec1e5ed43742a034b03a470dfdd2126c7ad0) |
| `sidecar/System.Globalization.Extensions.dll` | 15 KB | [`ce9bec51216ae2a7...`](https://www.virustotal.com/gui/file/ce9bec51216ae2a7b62b8c4c76b8767ee394b56a079144cdf6d1c636fcc29518) |
| `sidecar/System.Interactive.Async.dll` | 359 KB | [`ffac755159ef51c4...`](https://www.virustotal.com/gui/file/ffac755159ef51c4b119a6d68650d0e8e5d392d54bcfd27513cb30aa45393f0d) |
| `sidecar/System.IO.Compression.Brotli.dll` | 82 KB | [`8bfd12e4b2d4f760...`](https://www.virustotal.com/gui/file/8bfd12e4b2d4f760a716ecba8768518a3d8b15d928a16c696b046803be2a9121) |
| `sidecar/System.IO.Compression.dll` | 410 KB | [`54125287d05b5756...`](https://www.virustotal.com/gui/file/54125287d05b5756be2283bf7e687911525ba78f6f9a02cf061f12819664a035) |
| `sidecar/System.IO.Compression.FileSystem.dll` | 15 KB | [`6d8008659631783e...`](https://www.virustotal.com/gui/file/6d8008659631783e48a1e660e5cc2ac145661a7c6039874be79b7386f6fad7c0) |
| `sidecar/System.IO.Compression.Native.dll` | 916 KB | [`2410c8647ea90a1e...`](https://www.virustotal.com/gui/file/2410c8647ea90a1eec732d322780a7ad5c685d6a7febf89499bbcc64f6a36ac7) |
| `sidecar/System.IO.Compression.ZipFile.dll` | 98 KB | [`d33e34ffecb57a51...`](https://www.virustotal.com/gui/file/d33e34ffecb57a51c798d94fe2d094fb4366311940d9d1dfd861394fcff4bab0) |
| `sidecar/System.IO.dll` | 16 KB | [`a1dc011c9a03a72e...`](https://www.virustotal.com/gui/file/a1dc011c9a03a72ecfbbd758c1aecfec44ec60601b793230715229e359205b13) |
| `sidecar/System.IO.FileSystem.AccessControl.dll` | 102 KB | [`0cb35ca3655b8bd9...`](https://www.virustotal.com/gui/file/0cb35ca3655b8bd9a40cc69bd0649e1a28aec5dc2a55cfdf8dbf9c236e8728b3) |
| `sidecar/System.IO.FileSystem.dll` | 16 KB | [`607389b5e1df631e...`](https://www.virustotal.com/gui/file/607389b5e1df631e5fdf98d72d54c52b1c1626dcdc7102766abd068a4397c6ff) |
| `sidecar/System.IO.FileSystem.DriveInfo.dll` | 54 KB | [`c4f4bf1bd1d12893...`](https://www.virustotal.com/gui/file/c4f4bf1bd1d128935d3b7d170686abce9c0c4e27f2d1f06f4f0fab02ad2dc88d) |
| `sidecar/System.IO.FileSystem.Primitives.dll` | 15 KB | [`dbddafd39433863a...`](https://www.virustotal.com/gui/file/dbddafd39433863a987888bae5b01d3423f232a728e20ba8351874ba451fecf6) |
| `sidecar/System.IO.FileSystem.Watcher.dll` | 86 KB | [`905f1e7d51ae5b3d...`](https://www.virustotal.com/gui/file/905f1e7d51ae5b3d8b457ec77696d277593f72a0a529c1aa58e82f84ba7e85c3) |
| `sidecar/System.IO.IsolatedStorage.dll` | 86 KB | [`db493644026616cf...`](https://www.virustotal.com/gui/file/db493644026616cfd12bbb0642a0c2b3deef3f1a2d671e29460eea3a1caa7b74) |
| `sidecar/System.IO.MemoryMappedFiles.dll` | 82 KB | [`22ff8ae2c8d1cec8...`](https://www.virustotal.com/gui/file/22ff8ae2c8d1cec8ed1f91e7d81f733488422a059736f74b4044f5b39a1eb849) |
| `sidecar/System.IO.Pipelines.dll` | 186 KB | [`e8be292ccfdd68dd...`](https://www.virustotal.com/gui/file/e8be292ccfdd68ddf4c061235ee6890c66384592df4a29d65215ac5bc33c0d1c) |
| `sidecar/System.IO.Pipes.AccessControl.dll` | 16 KB | [`71dd12bb35b9c135...`](https://www.virustotal.com/gui/file/71dd12bb35b9c1357cef6e4d9c9b318e21d611153b243ade91028f00e9b2e4af) |
| `sidecar/System.IO.Pipes.dll` | 162 KB | [`93404bb5129a15b6...`](https://www.virustotal.com/gui/file/93404bb5129a15b6966a9727d2c3c0ce37a2bd3864e32b5375a74bb53fc02133) |
| `sidecar/System.IO.UnmanagedMemoryStream.dll` | 16 KB | [`9eb9e22061531d65...`](https://www.virustotal.com/gui/file/9eb9e22061531d651db0c3198754bab28b58d3b67ab55ba8dd1d9873642ca89c) |
| `sidecar/System.Linq.Async.dll` | 1.13 MB | [`9023bdc38c02823f...`](https://www.virustotal.com/gui/file/9023bdc38c02823fd86a2100350455ec0521fbef0267ad7e62ce38b3c44df608) |
| `sidecar/System.Linq.AsyncEnumerable.dll` | 1.23 MB | [`94db062725cb2842...`](https://www.virustotal.com/gui/file/94db062725cb28425224954527ccca4aa1729f7b5da3b1af46015a2e3e3c171a) |
| `sidecar/System.Linq.dll` | 682 KB | [`35d3db8aae98b0b8...`](https://www.virustotal.com/gui/file/35d3db8aae98b0b8f94e9f3d94dc823e29a729c41c50b007c7383603d6b422b4) |
| `sidecar/System.Linq.Expressions.dll` | 3.47 MB | [`46601d899f2a673e...`](https://www.virustotal.com/gui/file/46601d899f2a673e5e387339ae7463b5cbd03e386292b1683feec819c5a33f1d) |
| `sidecar/System.Linq.Parallel.dll` | 758 KB | [`c5b7d7d208d30fc0...`](https://www.virustotal.com/gui/file/c5b7d7d208d30fc02da8ee495e8b928c9591f327fb6f2d344b371100e0a9bfef) |
| `sidecar/System.Linq.Queryable.dll` | 178 KB | [`9087b4abc9fc1c98...`](https://www.virustotal.com/gui/file/9087b4abc9fc1c987379f6239a083190c51cb1528ea014812b32fe5aafb845c7) |
| `sidecar/System.Memory.dll` | 158 KB | [`2edc8f3bbc0e60ee...`](https://www.virustotal.com/gui/file/2edc8f3bbc0e60ee613a0d9d456846bfb2635118772b3d9d51c3cc116d5c5aa2) |
| `sidecar/System.Net.dll` | 17 KB | [`481f6cd2ab5df0c8...`](https://www.virustotal.com/gui/file/481f6cd2ab5df0c82f409f96cfe9e454b13894d2318ab004e3d7729445c2fb93) |
| `sidecar/System.Net.Http.dll` | 1.67 MB | [`49883dba5c99ffad...`](https://www.virustotal.com/gui/file/49883dba5c99ffad5e43230be4ee38566429d4e307f711058a8141c8b0904076) |
| `sidecar/System.Net.Http.Json.dll` | 126 KB | [`a802fe7a8c9f25fc...`](https://www.virustotal.com/gui/file/a802fe7a8c9f25fc85d2cb5950d6840cf614f82988a4c4d61bc25753288738af) |
| `sidecar/System.Net.HttpListener.dll` | 522 KB | [`4323344f89550aa0...`](https://www.virustotal.com/gui/file/4323344f89550aa03f58d4aa5f05357caa6c72011dee465541341486d58ab7a8) |
| `sidecar/System.Net.Mail.dll` | 466 KB | [`14484a168ae4aad7...`](https://www.virustotal.com/gui/file/14484a168ae4aad728766ea5220adb0b31208bd2b339cc1107b08a4cb39c31fa) |
| `sidecar/System.Net.NameResolution.dll` | 122 KB | [`0cf3e26e78305cd8...`](https://www.virustotal.com/gui/file/0cf3e26e78305cd8642b09d7f6ecc879904f53cd500645fc618441a1043b8476) |
| `sidecar/System.Net.NetworkInformation.dll` | 154 KB | [`565bc405b5f3d720...`](https://www.virustotal.com/gui/file/565bc405b5f3d720189a74ba1b7980dfb14dd5363ec79b6923d834573dce9f38) |
| `sidecar/System.Net.Ping.dll` | 90 KB | [`f2067509f102e0e4...`](https://www.virustotal.com/gui/file/f2067509f102e0e471c12562c7116124ffc9556a003baa40b96981935916b15e) |
| `sidecar/System.Net.Primitives.dll` | 206 KB | [`603da5e0255f11d4...`](https://www.virustotal.com/gui/file/603da5e0255f11d466647b4b73d746e87559d4b6e9890007b170e660012972a7) |
| `sidecar/System.Net.Quic.dll` | 350 KB | [`81a5513da8ad4788...`](https://www.virustotal.com/gui/file/81a5513da8ad478823db571f28e94f17a2526b738b82db3bffa5eb08a8ccaed7) |
| `sidecar/System.Net.Requests.dll` | 366 KB | [`46aafa741962c128...`](https://www.virustotal.com/gui/file/46aafa741962c12843e96a68eaaf20af2b12dacd574870fb830dc9b187da15ca) |
| `sidecar/System.Net.Security.dll` | 650 KB | [`de258a29818a70f1...`](https://www.virustotal.com/gui/file/de258a29818a70f18049b1d33d6bc7f28158d3c1836678b17516ac47a60abcb7) |
| `sidecar/System.Net.ServerSentEvents.dll` | 82 KB | [`e87d639de79c70ee...`](https://www.virustotal.com/gui/file/e87d639de79c70ee8437811b5f7b4a82f8af47934cd8aca339294f7444332e46) |
| `sidecar/System.Net.ServicePoint.dll` | 15 KB | [`3351cb1353f88833...`](https://www.virustotal.com/gui/file/3351cb1353f88833371b2e685abbf870e679cb1e71ac0136138690c2abdcce5e) |
| `sidecar/System.Net.Sockets.dll` | 518 KB | [`e8c79b6599f33e00...`](https://www.virustotal.com/gui/file/e8c79b6599f33e00ea57d7eeb05a6ac8a8378adae042f290a88a21c282936a78) |
| `sidecar/System.Net.WebClient.dll` | 162 KB | [`837d47cc0046eee0...`](https://www.virustotal.com/gui/file/837d47cc0046eee0b74b009f41c1ed934dea2da2eba8a5f04c43b621e364fd09) |
| `sidecar/System.Net.WebHeaderCollection.dll` | 62 KB | [`8aa72c4c376f039d...`](https://www.virustotal.com/gui/file/8aa72c4c376f039dd48efb25053ce9b78944b0c10bdbd7fbcb177e013799ba79) |
| `sidecar/System.Net.WebProxy.dll` | 42 KB | [`945414567ba7fb09...`](https://www.virustotal.com/gui/file/945414567ba7fb0905cfddfcd4875a46736b9e750c862176fa29d99d8bf73232) |
| `sidecar/System.Net.WebSockets.Client.dll` | 102 KB | [`a05268c53aa5560a...`](https://www.virustotal.com/gui/file/a05268c53aa5560a3707cc36ab7c34f78e10894fecf0550602dd883a40c3ec30) |
| `sidecar/System.Net.WebSockets.dll` | 238 KB | [`d52fd07f4b88903c...`](https://www.virustotal.com/gui/file/d52fd07f4b88903c1e2652c56712d66a5db00ed31b0c4a246becc95944083cef) |
| `sidecar/System.Numerics.dll` | 15 KB | [`1456bc505a07b2d8...`](https://www.virustotal.com/gui/file/1456bc505a07b2d80498aacd952d916b2fff2542623626896fa7b67a116c4a4e) |
| `sidecar/System.Numerics.Tensors.dll` | 506 KB | [`cd6878734a1fee6e...`](https://www.virustotal.com/gui/file/cd6878734a1fee6ecb50e3e151576cb7c7547bf2ddf5d480e95c01549e156c8c) |
| `sidecar/System.Numerics.Vectors.dll` | 16 KB | [`3e6bfa7b734e63cc...`](https://www.virustotal.com/gui/file/3e6bfa7b734e63cc67b919f1ef75601cc01dbd496c146e159ef61c5c5925adf5) |
| `sidecar/System.ObjectModel.dll` | 78 KB | [`705c9b2ecb2d4a5e...`](https://www.virustotal.com/gui/file/705c9b2ecb2d4a5ede420492483645bb71b53697faefe5e7165e1adbf76e3583) |
| `sidecar/System.Private.CoreLib.dll` | 15.28 MB | [`a4d81ebc7474e4a1...`](https://www.virustotal.com/gui/file/a4d81ebc7474e4a1fccfd8d7e56abbd12d396421c4447bfce25c3526f5479c2e) |
| `sidecar/System.Private.DataContractSerialization.dll` | 1.97 MB | [`6882776693e72938...`](https://www.virustotal.com/gui/file/6882776693e729380bab7ee778415eee44575c6655b187359c6ff8365efdc1da) |
| `sidecar/System.Private.Uri.dll` | 250 KB | [`e2f1fa1acba6fd32...`](https://www.virustotal.com/gui/file/e2f1fa1acba6fd328fcd213d031ef4993c69d510753254b8bb6bf4d484fd8452) |
| `sidecar/System.Private.Xml.dll` | 7.42 MB | [`b1c078417751112e...`](https://www.virustotal.com/gui/file/b1c078417751112edd7d83f0eb82947e8c849e79b44a143da6e6544734e293ff) |
| `sidecar/System.Private.Xml.Linq.dll` | 382 KB | [`0f4e874b77c8a77d...`](https://www.virustotal.com/gui/file/0f4e874b77c8a77daba39cc1cd204444c3f7e9310282a46b7de9742cb826e488) |
| `sidecar/System.Reflection.DispatchProxy.dll` | 74 KB | [`5fbab1a8a4a0a074...`](https://www.virustotal.com/gui/file/5fbab1a8a4a0a074d6359288ddc7a4a156ec55ca264f8aff3c58c7c05dcfcfa1) |
| `sidecar/System.Reflection.dll` | 16 KB | [`12733a1392e34f44...`](https://www.virustotal.com/gui/file/12733a1392e34f44454de4d3efe66bc2d44a738114284f2aa0a259fc176d0375) |
| `sidecar/System.Reflection.Emit.dll` | 298 KB | [`4ef779b618caa73f...`](https://www.virustotal.com/gui/file/4ef779b618caa73f26a89dd245e1d95d241960c6c3e444f1704b3af8a5c88398) |
| `sidecar/System.Reflection.Emit.ILGeneration.dll` | 16 KB | [`c5b90d115232745c...`](https://www.virustotal.com/gui/file/c5b90d115232745c7f53618b7909f0fb6a6d55c0ccb7989af11e4dc0588b3075) |
| `sidecar/System.Reflection.Emit.Lightweight.dll` | 16 KB | [`fc6f43c75ac4bc2a...`](https://www.virustotal.com/gui/file/fc6f43c75ac4bc2a8515853ced2c1f1857a55840eb408e963d90c0ce6767deda) |
| `sidecar/System.Reflection.Extensions.dll` | 15 KB | [`19d1f667054f2fb5...`](https://www.virustotal.com/gui/file/19d1f667054f2fb5be4930aa88f471a4a9b0155f2aa3068022817caa31c61bb3) |
| `sidecar/System.Reflection.Metadata.dll` | 1.10 MB | [`bab928a4eaa48c2f...`](https://www.virustotal.com/gui/file/bab928a4eaa48c2f117870afa6128444cbe05b0bc67166f3987eafdefe57c13a) |
| `sidecar/System.Reflection.Primitives.dll` | 16 KB | [`fba89634f472b88b...`](https://www.virustotal.com/gui/file/fba89634f472b88b45eed7a457d491f679fb48da7af21b2f65d3baabd6410d03) |
| `sidecar/System.Reflection.TypeExtensions.dll` | 42 KB | [`242fa6ac9b07c8e1...`](https://www.virustotal.com/gui/file/242fa6ac9b07c8e18ec84a099c1960936406c37e43c620547cdae390492a6195) |
| `sidecar/System.Resources.Reader.dll` | 15 KB | [`81a95a4376cbac73...`](https://www.virustotal.com/gui/file/81a95a4376cbac73546a5fc6f4d85fe3f2d20264de3ebfe7d1b040bc5fc89828) |
| `sidecar/System.Resources.ResourceManager.dll` | 16 KB | [`056113478712257c...`](https://www.virustotal.com/gui/file/056113478712257c495b71629443c607a9d66ccb75e29f9e6fadf9e9e1216baa) |
| `sidecar/System.Resources.Writer.dll` | 50 KB | [`b4d5391e84612d95...`](https://www.virustotal.com/gui/file/b4d5391e84612d95466aacce501baa0e2d14fa0005cef87f0c89c4f61baaa844) |
| `sidecar/System.Runtime.CompilerServices.Unsafe.dll` | 15 KB | [`2079ea68712ba28b...`](https://www.virustotal.com/gui/file/2079ea68712ba28b740c3d733be7eef3ecdea24b503901914a39bd00992a6df5) |
| `sidecar/System.Runtime.CompilerServices.VisualC.dll` | 30 KB | [`670ad7dab228b4a8...`](https://www.virustotal.com/gui/file/670ad7dab228b4a878768ddb532b346f8bcdeaa0d987a7540b8227e358303e93) |
| `sidecar/System.Runtime.dll` | 44 KB | [`9d746dce301ab142...`](https://www.virustotal.com/gui/file/9d746dce301ab142d3aeb16d5ee0fd988e5edeaa4cd8d88ce18ad1ca9f91237b) |
| `sidecar/System.Runtime.Extensions.dll` | 18 KB | [`7e0023173211eb6f...`](https://www.virustotal.com/gui/file/7e0023173211eb6f5419f4ae9ea98f339b8cad09aab1a36b783265fa4e33e2e4) |
| `sidecar/System.Runtime.Handles.dll` | 16 KB | [`c10b28b6259a3139...`](https://www.virustotal.com/gui/file/c10b28b6259a313972d341242a0ea28a359b28e90c33883da206f531410cd785) |
| `sidecar/System.Runtime.InteropServices.dll` | 110 KB | [`90070cc512414889...`](https://www.virustotal.com/gui/file/90070cc5124148898fa246690b1cbcd101c620bcb11f4d53b2cd6228260a6242) |
| `sidecar/System.Runtime.InteropServices.JavaScript.dll` | 50 KB | [`98c1efa82c41e4dd...`](https://www.virustotal.com/gui/file/98c1efa82c41e4dd58d4aa52c0db7041de77b2d8a28f81524dc242a7e54fcfa7) |
| `sidecar/System.Runtime.InteropServices.RuntimeInformation.dll` | 15 KB | [`111715331d4307a6...`](https://www.virustotal.com/gui/file/111715331d4307a606d6bc2d73f653ecad3b78caf628a2e5fdccdd73ae34dbd2) |
| `sidecar/System.Runtime.Intrinsics.dll` | 17 KB | [`5a158b76ca0e636a...`](https://www.virustotal.com/gui/file/5a158b76ca0e636afafd8ce0c842ad5ba147ce8fa1ba842a80ff7a7e2aa9bc59) |
| `sidecar/System.Runtime.Loader.dll` | 16 KB | [`3157195885c5139a...`](https://www.virustotal.com/gui/file/3157195885c5139ae65131755a15fa2086b39208ef52576e87fce425fd805956) |
| `sidecar/System.Runtime.Numerics.dll` | 350 KB | [`553e95eb14e7b4c4...`](https://www.virustotal.com/gui/file/553e95eb14e7b4c4e782af4fc56b960430ff307a28aabb833d8ee6948dc18700) |
| `sidecar/System.Runtime.Serialization.dll` | 17 KB | [`f8a4ebd993641fc7...`](https://www.virustotal.com/gui/file/f8a4ebd993641fc79a4782c83128320b9637a07b41a7bc6c0e18e7442ab9c3a2) |
| `sidecar/System.Runtime.Serialization.Formatters.dll` | 122 KB | [`8f5ba5e81df658d6...`](https://www.virustotal.com/gui/file/8f5ba5e81df658d62f74a8eb2cf53e08cf673bfc606ed5205430e2499a3f742f) |
| `sidecar/System.Runtime.Serialization.Json.dll` | 16 KB | [`5151ab2a5949f2c1...`](https://www.virustotal.com/gui/file/5151ab2a5949f2c128cd441d099a3b52293044ade8ed228e223754fdf6c68d3d) |
| `sidecar/System.Runtime.Serialization.Primitives.dll` | 38 KB | [`b1a204b360b8a621...`](https://www.virustotal.com/gui/file/b1a204b360b8a62194e4db6e333a2e197532e6590f4598d1c8a134083c7264e9) |
| `sidecar/System.Runtime.Serialization.Xml.dll` | 17 KB | [`408794a8979a02bc...`](https://www.virustotal.com/gui/file/408794a8979a02bc50005fbbaa4895890bd269e57e074b7a7cdc6689f4537de5) |
| `sidecar/System.Security.AccessControl.dll` | 222 KB | [`1a70af5e3728eef4...`](https://www.virustotal.com/gui/file/1a70af5e3728eef487ffb5abb4787511f89c6c1c190eecfe9bd836bf1c47b32e) |
| `sidecar/System.Security.Claims.dll` | 98 KB | [`36285c72fc83bad5...`](https://www.virustotal.com/gui/file/36285c72fc83bad58e4be8d4e76fe9a17224690a01e5fdf1d2d96b97f142e39b) |
| `sidecar/System.Security.Cryptography.Algorithms.dll` | 17 KB | [`deb298cf579ed2b4...`](https://www.virustotal.com/gui/file/deb298cf579ed2b44aa7874aa4e16b09c828727a45f3b6f8c677b330667a65c2) |
| `sidecar/System.Security.Cryptography.Cng.dll` | 16 KB | [`aa951cb97ee10393...`](https://www.virustotal.com/gui/file/aa951cb97ee10393a7121992989c98d88f5e0b98fed0ecbf495bb0e15dd90d3f) |
| `sidecar/System.Security.Cryptography.Csp.dll` | 16 KB | [`99bbbf09f1219882...`](https://www.virustotal.com/gui/file/99bbbf09f1219882b66ec70264b4fc4f2664bc71e9dd6b2dbf2f8e5e9601a4e6) |
| `sidecar/System.Security.Cryptography.dll` | 2.43 MB | [`b7f8e7ace9c723ea...`](https://www.virustotal.com/gui/file/b7f8e7ace9c723ea66b8787278d863880a65ea6d19d7385dd9662372c511e6b2) |
| `sidecar/System.Security.Cryptography.Encoding.dll` | 16 KB | [`88d0018a9ca6621d...`](https://www.virustotal.com/gui/file/88d0018a9ca6621d91a45921effed726822df59a6aae425d4f5aa42843cd4145) |
| `sidecar/System.Security.Cryptography.OpenSsl.dll` | 16 KB | [`129a45fe68d5efa2...`](https://www.virustotal.com/gui/file/129a45fe68d5efa2dfb498ad62ee1cc182cbccc5429a2788e8d5cbb244a96db3) |
| `sidecar/System.Security.Cryptography.Primitives.dll` | 16 KB | [`12ebb8ba3010a20a...`](https://www.virustotal.com/gui/file/12ebb8ba3010a20af0b60a155e8c983e6eec18ddc9ca94cdeb3699b46d34308d) |
| `sidecar/System.Security.Cryptography.X509Certificates.dll` | 17 KB | [`16460f6e21b16fa5...`](https://www.virustotal.com/gui/file/16460f6e21b16fa52d82b1bd4ad1a75a8f3ca9996680b427ce30cdc8e8ee474e) |
| `sidecar/System.Security.dll` | 18 KB | [`07c9de1dc126080f...`](https://www.virustotal.com/gui/file/07c9de1dc126080f8e4d687b45ccac32c3ffe31e398796272138e8f09bfb09a8) |
| `sidecar/System.Security.Principal.dll` | 15 KB | [`e67314432370eda7...`](https://www.virustotal.com/gui/file/e67314432370eda775236b13e3d1b5018a60401eeaa50a9a6d52ba7fb75aecee) |
| `sidecar/System.Security.Principal.Windows.dll` | 174 KB | [`6f74f292955eae9f...`](https://www.virustotal.com/gui/file/6f74f292955eae9f4d7b68cbb6d8bb54756abfa9c0a75325c3d890ccdf580d28) |
| `sidecar/System.Security.SecureString.dll` | 16 KB | [`6d75621dce36223d...`](https://www.virustotal.com/gui/file/6d75621dce36223d9ce99254fb618d479e531bcfdc2ffb9f71c50e0a90ed2e34) |
| `sidecar/System.ServiceModel.Web.dll` | 17 KB | [`d8354b9f238b8a33...`](https://www.virustotal.com/gui/file/d8354b9f238b8a33c2cb04c0093ef921ebb8165b212759411d251a8b97870c57) |
| `sidecar/System.ServiceProcess.dll` | 16 KB | [`77441b2e21dec424...`](https://www.virustotal.com/gui/file/77441b2e21dec42412e98673f4b43bbfe6f4e6851c13904308784b05f30cd9d6) |
| `sidecar/System.Text.Encoding.CodePages.dll` | 842 KB | [`ea29a4692e96f2c2...`](https://www.virustotal.com/gui/file/ea29a4692e96f2c2752a6cf2bd669552030cb4668d02dbbd6125cb2a389a213b) |
| `sidecar/System.Text.Encoding.dll` | 16 KB | [`ee070faf63415e58...`](https://www.virustotal.com/gui/file/ee070faf63415e584c9610a04cea9baa3364daf34645b8b36383bddd04a5a37d) |
| `sidecar/System.Text.Encoding.Extensions.dll` | 16 KB | [`42b272c4c47f90cf...`](https://www.virustotal.com/gui/file/42b272c4c47f90cf3f5f4fd92c77b90675df527805121acef16f5f31fc06ef2a) |
| `sidecar/System.Text.Encodings.Web.dll` | 122 KB | [`802d60772245fb8d...`](https://www.virustotal.com/gui/file/802d60772245fb8d0bf9e357f86fd64fabbd6de88d3f2a0f372372cd3fa5eb35) |
| `sidecar/System.Text.Json.dll` | 1.80 MB | [`618c5e2bcd047d8b...`](https://www.virustotal.com/gui/file/618c5e2bcd047d8bd24979c4fb0c8053a8feef4e7eaaa896a821ce0c1e9faa29) |
| `sidecar/System.Text.RegularExpressions.dll` | 1,010 KB | [`1b0f95cc9c5b7cca...`](https://www.virustotal.com/gui/file/1b0f95cc9c5b7ccac24a7c037ccb107efabf6eee2c7ea211dd08fbfadf24cbda) |
| `sidecar/System.Threading.AccessControl.dll` | 74 KB | [`66abcacd3b6991d0...`](https://www.virustotal.com/gui/file/66abcacd3b6991d05b4485cb2251890084ef0ea5a7bbf62ef11adb951105820e) |
| `sidecar/System.Threading.Channels.dll` | 154 KB | [`3e7d5104e35e3ed4...`](https://www.virustotal.com/gui/file/3e7d5104e35e3ed4668729d96eec3eeb9f13ecede53c90ac99c19bd440fe5796) |
| `sidecar/System.Threading.dll` | 82 KB | [`b41305e5388ab57c...`](https://www.virustotal.com/gui/file/b41305e5388ab57cd8fe4ef0aebb87eb52b673e5949594782f2384f568ad2671) |
| `sidecar/System.Threading.Overlapped.dll` | 16 KB | [`d9b5922df473f386...`](https://www.virustotal.com/gui/file/d9b5922df473f386069431fe3386297880bb320c3613a46d809857216d68cf3f) |
| `sidecar/System.Threading.Tasks.Dataflow.dll` | 462 KB | [`534f1970647ed828...`](https://www.virustotal.com/gui/file/534f1970647ed82890b90ef69bef5f1787dd4886a93852515865162b43c96aa3) |
| `sidecar/System.Threading.Tasks.dll` | 17 KB | [`54f1e98ed0027833...`](https://www.virustotal.com/gui/file/54f1e98ed0027833c0bc4c7e3d8e1cceb562358e1fe9e6101a11dfcd25279dbe) |
| `sidecar/System.Threading.Tasks.Extensions.dll` | 16 KB | [`c870d60ed28aa130...`](https://www.virustotal.com/gui/file/c870d60ed28aa13061a2aea097249b9e44fd26f1f022fc52b31baab670965cbc) |
| `sidecar/System.Threading.Tasks.Parallel.dll` | 130 KB | [`361acb6dac5c431a...`](https://www.virustotal.com/gui/file/361acb6dac5c431ab105ff554e0618209748b3e0135cb4805d9aab7bb9ea6a16) |
| `sidecar/System.Threading.Thread.dll` | 16 KB | [`ddef5a26c57d5e5f...`](https://www.virustotal.com/gui/file/ddef5a26c57d5e5ff574ac237613ab524d6b8cb6b33a7759d25b8378c27fea43) |
| `sidecar/System.Threading.ThreadPool.dll` | 16 KB | [`59d6a91ac53a1a4f...`](https://www.virustotal.com/gui/file/59d6a91ac53a1a4f63f5a3d7266d47cc93892a47530e857c6344f127394a1c4d) |
| `sidecar/System.Threading.Timer.dll` | 15 KB | [`c42a0081624041c9...`](https://www.virustotal.com/gui/file/c42a0081624041c9c7d2bff7fc808c5856c6620488dba197d0687682a29ed106) |
| `sidecar/System.Transactions.dll` | 17 KB | [`eb06b5b7a47e982d...`](https://www.virustotal.com/gui/file/eb06b5b7a47e982dee9a9791e1710666400bcd9a49d752df7af19a040e6104f0) |
| `sidecar/System.Transactions.Local.dll` | 626 KB | [`03cf0584b0b7847b...`](https://www.virustotal.com/gui/file/03cf0584b0b7847bc2b676196e289d27be46a0a1bd863a99ee05b08203119927) |
| `sidecar/System.ValueTuple.dll` | 16 KB | [`54329aab5d1cfeee...`](https://www.virustotal.com/gui/file/54329aab5d1cfeeeee480de645db313f26acd4dec0356c995706f10ead1e0a72) |
| `sidecar/System.Web.dll` | 15 KB | [`3af2cdd3753f03e3...`](https://www.virustotal.com/gui/file/3af2cdd3753f03e3c55dfe9f9e9b784e6af6d33a77d4a0eb1124bb73997bdeb9) |
| `sidecar/System.Web.HttpUtility.dll` | 62 KB | [`f7b5948cce0c9d8b...`](https://www.virustotal.com/gui/file/f7b5948cce0c9d8b1a3c8bd12f5ce6a04d511b2610dc53706ac63225ce12a5a2) |
| `sidecar/System.Windows.dll` | 16 KB | [`d697bf597889f0d2...`](https://www.virustotal.com/gui/file/d697bf597889f0d20d3ca7fe0bf8b6d5102ef2853fd2cea871181ff17030632a) |
| `sidecar/System.Xml.dll` | 23 KB | [`a26fc0115cd0b2f6...`](https://www.virustotal.com/gui/file/a26fc0115cd0b2f60584fc11c72a0df009fff6dedd321a91f31202d39882c780) |
| `sidecar/System.Xml.Linq.dll` | 16 KB | [`373de9c19748d799...`](https://www.virustotal.com/gui/file/373de9c19748d7992c77707f02c06c0f553f559baccbba31ca3d31d625350154) |
| `sidecar/System.Xml.ReaderWriter.dll` | 22 KB | [`b6d33140f85ecdb0...`](https://www.virustotal.com/gui/file/b6d33140f85ecdb086526d38aad3f0e99c50e373754c540540ce4fb7ea931cea) |
| `sidecar/System.Xml.Serialization.dll` | 16 KB | [`5a104b3ecd7bf6a5...`](https://www.virustotal.com/gui/file/5a104b3ecd7bf6a5f1fd2a877c0297d801c80770d894483548160043a3633be7) |
| `sidecar/System.Xml.XDocument.dll` | 16 KB | [`7482138978681c6f...`](https://www.virustotal.com/gui/file/7482138978681c6f538fd547cf5f07b2d1fd06bd969098e7a5e8e4d84e63eb43) |
| `sidecar/System.Xml.XmlDocument.dll` | 16 KB | [`ca60ece16aa11d71...`](https://www.virustotal.com/gui/file/ca60ece16aa11d71d73cf59bdac1a9f606b83f0bf63a99134d99861fa00da656) |
| `sidecar/System.Xml.XmlSerializer.dll` | 18 KB | [`1c9b14b89b623906...`](https://www.virustotal.com/gui/file/1c9b14b89b62390694ddd823c07e6bf1f62f8da2aa040831dfff6cbac583b3cf) |
| `sidecar/System.Xml.XPath.dll` | 16 KB | [`87ad820665febcfc...`](https://www.virustotal.com/gui/file/87ad820665febcfcd24bfafcfee1202968d10094494a3698b578982da7759ad7) |
| `sidecar/System.Xml.XPath.XDocument.dll` | 30 KB | [`e476b42ef8f2e6b8...`](https://www.virustotal.com/gui/file/e476b42ef8f2e6b8caf2005cef88651930ae91d7cb62aab56c867e5aefa8dcd1) |
| `sidecar/WindowsBase.dll` | 16 KB | [`ed7980cea1434af7...`](https://www.virustotal.com/gui/file/ed7980cea1434af712ef4919b5cceefbcbc274ef613cbfb95675596b339b83b5) |

</details>

## Nothing else ships

The archive holds 234 files. The tables above cover 225 of them: every executable, every native library,
and both model files. The remaining 9 carry no code and are listed here so the accounting is complete.

| File | Size | SHA256 |
|---|---|---|
| `characters/linus.json` | 1 KB | `d4ea22ee46af69f20cb6835c317bbbb603a0f8777fbfbba451ad57ce227ef38f` |
| `config.json` | 392 B | `9766d45e85c647aeb6f490a14a0f00c9763e3e449c15a297bd59d1e6127a60a9` |
| `LICENSE` | 11 KB | `cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30` |
| `LICENSE-LFM.txt` | 11 KB | `622215e455bf5452a7edd091324c49195c119ed8b901238fbf331a0675dd9dc5` |
| `manifest.json` | 457 B | `62c22575e38041296900cafe68340b64b590e8a6fe58fa9ea682c1b953f47881` |
| `NOTICE` | 2 KB | `7ab8b1454d60fa117372769b323a6bb5080600d8954e21686ef68d9f7a2d4f50` |
| `README.txt` | 2 KB | `ded7e6f168dc9d417d7e9eaa41ff36a74033582eb63a78d36e11285a74b357b3` |
| `sidecar/ChattyValley.Sidecar.deps.json` | 35 KB | `958e580603d1f15311e1dc53c3192212bd1499dede54d6030b04a65a2417a3ea` |
| `sidecar/ChattyValley.Sidecar.runtimeconfig.json` | 373 B | `263b2814509aec033d6b59e3d74a7abe48e926ad17cf44963e80612e0e401d25` |
