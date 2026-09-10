# ADP.APP.App

Aplicación desarrollada en **C# / .NET**.

Este documento describe los pasos necesarios para restaurar dependencias, compilar, ejecutar y publicar la aplicación.

## Requisitos

Antes de comenzar, asegúrate de tener instalado:

* .NET SDK compatible con el proyecto.
* Git.
* Visual Studio o Visual Studio Code, opcionalmente.

Para verificar la instalación de .NET:

```bash
dotnet --version
```

Para verificar los SDK instalados:

```bash
dotnet --list-sdks
```

## Restaurar dependencias

Desde la raíz del repositorio, ejecutar:

```bash
dotnet restore
```

## Ejecutar la aplicación

Para ejecutar el proyecto directamente en modo desarrollo:

```bash
dotnet run --project ADP.APP.App
```

También puedes especificar el archivo del proyecto:

```bash
dotnet run --project ADP.APP.App/SysAdm.csproj
```

## Compilar la aplicación

Para compilar el proyecto:

```bash
dotnet build --project ADP.APP.App
```

> Si `dotnet build --project` no es compatible con la versión del SDK utilizada, ejecuta el comando indicando directamente el archivo `.csproj`:

```bash
dotnet build ADP.APP.App/SysAdm.csproj
```

### Compilación en Release

Para generar una compilación optimizada para producción:

```bash
dotnet build ADP.APP.App/SysAdm.csproj -c Release
```

Los archivos generados normalmente se encuentran en:

```text
ADP.APP.App/bin/Release/
```

## Publicar la aplicación

Para generar una versión publicable de la aplicación en modo `Release`:

```bash
dotnet publish ADP.APP.App -c Release --self-contained true -o "ADP.APP.App/bin/release/SysAmd"
```

### Parámetros utilizados

| Parámetro               | Descripción                                                       |
| ----------------------- | ----------------------------------------------------------------- |
| `dotnet publish`        | Genera los archivos necesarios para distribuir la aplicación.     |
| `ADP.APP.App`           | Proyecto que se desea publicar.                                   |
| `-c Release`            | Utiliza la configuración `Release`.                               |
| `--self-contained true` | Incluye el runtime de .NET necesario para ejecutar la aplicación. |
| `-o`                    | Define el directorio donde se colocarán los archivos publicados.  |

El resultado de la publicación se encontrará en:

```text
ADP.APP.App/bin/release/SysAmd
```

## Ejemplo de flujo completo

Desde la raíz del proyecto:

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build ADP.APP.App/SysAdm.csproj -c Release

# Ejecutar en desarrollo
dotnet run --project ADP.APP.App

# Publicar
dotnet publish ADP.APP.App -c Release --self-contained true -o "ADP.APP.App/bin/release/SysAmd"
```

## Estructura esperada

```text
/
├── ADP.APP.App/
│   ├── SysAdm.csproj
│   ├── Program.cs
│   ├── ...
│   └── bin/
│       └── release/
│           └── SysAmd/
├── README.md
└── ...
```

## Distribución

Después de ejecutar `dotnet publish`, el directorio:

```text
ADP.APP.App/bin/release/SysAmd
```

contendrá los archivos necesarios para distribuir y ejecutar la aplicación.

Al utilizar:

```bash
--self-contained true
```

la publicación incluye el runtime de .NET correspondiente, por lo que no es necesario instalar el runtime de .NET por separado en el equipo destino.

## Limpieza del proyecto

Si necesitas eliminar los archivos generados anteriormente, puedes ejecutar:

```bash
dotnet clean
```

También puedes eliminar manualmente los directorios:

```text
bin/
obj/
```

y posteriormente restaurar y compilar nuevamente:

```bash
dotnet restore
dotnet build ADP.APP.App/ADP.APP.App.csproj -c Release
```

## Notas

* Ejecuta los comandos desde la raíz del repositorio.
* Verifica que la versión del .NET SDK instalada sea compatible con el proyecto.
* La configuración `Release` debe utilizarse para generar una versión destinada a distribución o producción.
* Antes de distribuir una nueva versión, se recomienda realizar una compilación limpia y validar la publicación generada.
