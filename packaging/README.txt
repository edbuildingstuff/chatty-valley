Chatty Valley (early access)
On-device AI dialogue for Stardew Valley. https://www.ertas.ai

WHAT THIS IS
Talk to Linus and he talks back, in his own voice, from an AI model running
entirely on your own computer. No cloud, no API key, no account, no server.
Linus is the first villager. More of Pelican Town is coming.

INSTALL
1. Install SMAPI 4.0.0 or newer from https://smapi.io
2. Unzip this whole ChattyValley folder into your Stardew Valley Mods folder.
3. Launch the game through SMAPI and load your save.
4. Stand near Linus and press C.

WHY THERE IS AN EXE IN THE SIDECAR FOLDER
Stardew runs on .NET 6, and the .NET binding for the inference library needs
.NET 10, so the model cannot run inside the game process. It runs in a small
companion program that the mod starts and stops for you. You never launch it
or configure it. The whole mod is open source and you can read exactly what it
does: https://github.com/edbuildingstuff/chatty-valley

IF SOMETHING GOES WRONG
Chatty Valley never writes to your save, friendship, quests, or mail. If the
model cannot start, free chat is disabled and the game plays exactly as normal.
Check the SMAPI console for a line starting with "Chatty Valley".

SETTINGS
Edit config.json in this folder. ChatKey changes the chat button. Temperature
is tuned to 0.35 and we recommend leaving it there. Setting ChatLogEnabled to
true writes conversation transcripts to chat-logs/ for bug reports.

Gpu controls where replies run: "auto" (default) uses a dedicated NVIDIA or
AMD graphics card with at least 2 GB of memory if one is found, "off" forces
CPU, and "on" forces GPU. The SMAPI console tells you which one is active.

LICENCE
The mod is Apache 2.0 (see LICENSE and NOTICE). Free to use, fork and ship.
The bundled model weights are separate: LFM2.5-1.2B-Instruct under the LFM Open
License v1.0 (see LICENSE-LFM.txt), which limits commercial use to entities
under 10 million USD annual revenue.

CREDITS
ConcernedApe for Stardew Valley. Pathoschild and the SMAPI project. The
llama.cpp and LLamaSharp projects. Liquid AI for the LFM2.5 base model.
Built by Edward Xi Yang (edbuildingstuff), Ertas AI. https://www.ertas.ai
Not affiliated with or endorsed by ConcernedApe.
