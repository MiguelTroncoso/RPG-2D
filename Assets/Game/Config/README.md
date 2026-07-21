# Configuración futura

Esta carpeta queda reservada para definiciones estáticas del juego que deban editarse sin mezclar reglas de dominio ni estado de una partida.

En H2 no contiene `ScriptableObject`, balance, enemigos, inventario ni configuración de red. Las constantes estructurales del greybox viven en `Assets/Game/Domain/Constants` para que el dominio y las herramientas de editor compartan el mismo contrato.
