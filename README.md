# ffx - External

Ferramenta externa em C# para manipulação de memória de processos como o **HD-Player** (emulador Android).  
Desenvolvido com `TMemory.cs` e permite aplicar cheats como *aim assist*, *no recoil* e outros via scanner de padrões AoB.

---

## Requisitos

- Windows
- Visual Studio 2022 ou superior
- .NET Framework 4.x
- Emulador **HD-Player** em execução

---

## Como usar

1. Compile o projeto (`ffx.sln`) no Visual Studio.
2. Execute o `ffx.exe` com o HD-Player aberto.
3. Use os atalhos no teclado para ativar comandos:
   - `F8`: Aim Assist
   - `ESC`: Sair
4. Para adicionar novos comandos, siga o guia abaixo.

---

## Como adicionar novos comandos

Você pode adicionar comandos personalizados ao projeto editando o arquivo `Program.cs`.

##  Exemplo: No Recoil (Ativação e Desativação)
oq e no recoil "Remove o recuo das armas durante os disparos"

```csharp
// # No Recoil - Atalho F7
private static Dictionary<long, string> originalNoRecoilValues = new Dictionary<long, string>();

public async static void NoRecoil()
{
    try
    {
        Int32 proc = Process.GetProcessesByName("HD-Player")[0].Id;
        MemLib.OpenProcess(proc);

        var addresses = await MemLib.AoBScan2(
            "30 48 2D E9 08 B0 8D E2 02 8B 2D ED 00 40 A0 E1 38 01 9F E5 " +
            "00 00 8F E0 00 00 D0 E5 00 00 50 E3 06 00 00 1A 28 01 9F E5 " +
            "00 00 9F E7 00 00 90 E5", true);

        if (addresses == null || !addresses.Any())
        {
            Console.WriteLine("No Recoil pattern not found!");
            return;
        }

        string replacement =
            "00 00 A0 E3 1E FF 2F E1 02 8B 2D ED 00 40 A0 E1 38 01 9F E5 " +
            "00 00 8F E0 00 00 D0 E5 00 00 50 E3 06 00 00 1A 28 01 9F E5 " +
            "00 00 9F E7 00 00 90 E5";

        foreach (var addr in addresses)
        {
            string original = MemLib.ReadAoB(addr, replacement.Split(\' \').Length);
            originalNoRecoilValues[addr] = original;
            MemLib.WriteAoB(addr, replacement);
        }

        Console.WriteLine("Recoil applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying in Recoil: {ex.Message}");
    }
}

public static void DisableNoRecoil()
{
    try
    {
        foreach (var entry in originalNoRecoilValues)
        {
            MemLib.WriteAoB(entry.Key, entry.Value);
        }

        originalNoRecoilValues.Clear();
        Console.WriteLine("In the deactivated recoil.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error Disable in Recoil: {ex.Message}");
    }
}
```
---

## 📂 Estrutura do Projeto

```
📁 ffx
 ├── Program.cs         <- principal aki
 ├── TMemory.cs         <- repo da memória
 ├── App.config
 ├── ffx.csproj
 └── ffx.sln
```

---

