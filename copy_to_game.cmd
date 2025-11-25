@echo off
set Name=NeedForCrown
copy /y bin\Debug\netstandard2.1\%Name%.dll "E:\Local\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Mods\%Name%\" && echo OK
pause
