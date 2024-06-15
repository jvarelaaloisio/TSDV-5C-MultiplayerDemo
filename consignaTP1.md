### Parcial Multiplayer
El parcial constara de una demostración en red lan con 2 PCs en Image y defensa del código in situ.

Las implementaciones deben hacerse con las técnicas utilizadas en clase, no utilizando librerías de terceros.

Se tendrá en cuenta la calidad del código entregado.

Se corregirá el código que este pusheado al repositorio, hasta el día anterior a la fecha del parcial.

Deben tener una build subida como release en su repositorio para la corrección.

#### ondición mínima de aprobación (0 - 4)

- [ ] Todo lo entregado deberá estar alojado en un repositorio GIT público, de lo contrario, la calificación será 0.

- [ ] El parcial deberá constar de una demo jugable de dos players moviéndose en pantalla, siendo capaces de disparar y con una consola de chat funcional.

- [ ] La partida puede durar un máximo de 3 minutos, en cuanto el tiempo termine el servidor se cierra, notificando antes quien estuvo más cerca de ganar a los usuarios.

- [ x ] Los clientes tienen un nombre único al entrar en la partida, si el nombre ya está en uso la conexión es rechazada explicando el motivo.

- [ ] La partida puede tener de 2 a 4 jugadores, en el momento que hay dos jugadores en el servidor comienza una cuenta atrás de 2 minutos en el servidor, tras esos dos minutos la partida comenzará con los jugadores que tenga.

- [ ] Cada jugador tiene 3 golpes de vida, cuando su vida llega a 0 el servidor lo kickea

- [ ] Los mensajes deben de implementar checksum, si el mensaje está corrupto no se despacha.

#### Condición mínima para promoción (4 - 7)

- [ ] Los mensajes de posición se ordenan.

- [ ] Los mensajes de consola son no descartables.

- [ ] Los mensajes de consola son ordenables.

- [ ] Los comandos de disparo son no descartables.

- [ ] En caso de que un jugador pierda la conexión, será expulsado tras 5 segundos.

- [ ] El servidor y los clientes mantienen el historial de mensajes no descartables durante 15 segundos.

#### Examen completo (7 - 10)

- [ ] El juego debe tener animaciones que se actualicen en red.

- [ ] El juego debe tener triggers de sonidos.

- [ ] Rollback y reconciliación en movimiento utilizando interpolaciones lineales.

- [ ] El servidor debe ser una build de servidor dedicado y tiene que funcionar sin errores.