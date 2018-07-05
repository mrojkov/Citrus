set "dir=%~dp0"

for /F "tokens=*" %%A in (%~1) do (
  if /I "%%~xA" equ ".png" (
    if exist %%A (
      "%dir%..\Citrus\Orange\Toolchain.Win\PngOptimizerCL.exe" --KeepPixels "%%A"
    )
  )
)
