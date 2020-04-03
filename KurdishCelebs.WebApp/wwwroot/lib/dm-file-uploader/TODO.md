X FECHA EN DEBUG
X FILESIZE
X MARGENES
X MEJORAR PROGRESS BAR
X IMPLEMENTAR MEJOR BACKEND.PHP
X ver que estilos de .dm-uploader .btn son necedsarios y cuales no
X README.MD DENTRO DE DEMO y comentariocabezal en backend.php
X En releaseEvents() hay codigo widget.element.off duplicado
X Juntar los errores en onFileError; onValidationError

DEMOS MG:
X update nombre de functiones de main.js: ui_addlog... etc, todas ui_*
X quitar palabras 'addedd to queue' DE LOS DEMOS / REVISAR CONFIG
X REVISAR LINKS A SOURCE DE CADA EJEMPLO
X NO SE ESTA QUITANDO EL MENSAJE 'NO FILES UPLOADED' DEL FILE LIST CUAND SE AGREGA EL PRIMERO , AHORA USA MUN MEDIA LISTA COPIA DE DANIELMG
X RE ORGANIZAR LOS NUEVOS ESTILOS EN LOS DEMOS DE DANIELMG BASIC.CSS Y MAIN CSS. ALGUND E ESOS SE VA Y ENTRA DMULPADER.MIN.CSS

PLUGIN:
X restructurar projecto
X update demos de repo con los cambios al progress bar UI functions
X onUploadComplete

X Cancel en realidad deberia detener aquellos uploading nada mas
  X revisar cancelAll caundo uso queue: this.queuePos = this.queue.length - 1; ... de veras quiero poner al final??
    X al cancellar for in all...  cancell() checkear que el i sea <= queuepOSTT?? IF QUEUE RUNNING
X CONTRIBUITING.MD
X Verficiar si el onComplete se llama si se ha cancelado el upload(no deberia)
X Verificar la licedncia este acorde a https://opensource.org/licenses/MIT
X revisar que bower.json y package.json contengan los nombres correctos, al igual que el projecto en github
X Update blog demos the new dm-uploader.min.js: El demo images deberia tener un validador de extension por ID Y EL DIALOGO CODE SOURCE
X onDocumentDragLeave Triggers cuando se dropea un archivo???/ deberia
X onNewFile debere llamar un continue, NO un return
X onBeforeUpload is a problem
X EMOVER 'FILE DE LOS CALLBACKS', AGREGARLO ES REALMENTE INNIECESARIO

X changelog.MD
X rEADME.md
  X typos
  X onUploadError documentation of xhr and status
  X? build status
  X? npm package version,
  X? bower package version

- ACTUALIZAR DEMOS DE DANIELMG.ORG

- redirigir demos viejas a los nuevos

- Testing!!!!!!!!

- RELEASE


FUTURE
- DEFAULT SETTINGS BE NOW MODIFICABLE
  - CHECK https://learn.jquery.com/jquery-ui/widget-factory/why-use-the-widget-factory/
- USE EVENTS AND HANDLERS TRIGGERS, NOT BE SETTINGS
- EL CONCOMPLETE SE LLAMA CUANDO QUEDAN PENDIENTES, AUQNUE NO SE ESTEN PROCESANDO
  ESTO PUEDE Q SEA EL COMPORTAMIENDO CORRECTO, O TALVEZ SEA RARO
- Cuando esta corriendo un queue, el start no inicia u uploader an el futuro
  - no se puede iniciar un archivo manualmente cuando el queue esta corriendo
- el iniciar global deberia (?) iniciar los pendientes
- Cool rproject example: https://github.com/showdownjs/showdown
- PROBLEMA: Que sucede cuando usando queue y noauto:
  - AGREGAR AL MENOS 3 ARCHIVOS
  - inicia un archiv manualmente (EL SEGUNDO)
  - inicia el queue
  - termina el manual, Y PASA AL SIGUIENTE???
- Can't 'start' files using the method: "start" (uh) when there is a queue currently running. More than an issue this is kinda a mix of features that don't work great together. Most likely this will be fixed during some code rewrite / refactoring / cleanup; doing it right now it may lead to some ugly code / function flags etc.
- Option hookDocument needs a review.
- Probably join validation callbacks into one like: onValidationError. This may imply having a hardcoded message and the plugin currently doen't have a i18n system.
- onBeforeUpload REGRESSION!


  X plugins.jquery.com / dmuploader.jquery.json
  X demos danielmg.org
    X basic
    X single upload ?
    X controls example
    X control queue
    X autoupload
    X image preview ?
  X npm.com
  
  X bower.io ?

Major changes before 0.2.0
- Review start(), cancel() and 'auto' option
  - Don't make the ID mandatory
    - start(): Without the ID parameter it will check if there is
      no queue running => start it!
    - cancel(): Without the ID parameter will stop que queue (if running),
      and call cancel() for any uploading files
    
  - Remove cancellAll()
    - It will work the same as cancel() with no arguments
    
  - Review the 'auto' option, and how the queue is processed with the
    new start() and cancell() from previous points
  - Also, before start the upload of a file coming from queue check
    that is  not uploading already.
  - Review new features added from pull requests and how can be integrated
    to the new version.

  - Check if some each() are necesary during creation!

---

Main changes
 - validators now are  !== null
 - extraData accepts now accepts a callback
 - dragenter ondragleave ondrop
 - fllback no mseesage
 - new dnd option

 onError tiene mas opciones

X cfunction initDnD: function is()
X checquear si el tag es un input file, sino es un div etc
X agregar file blob data a los callbacks de archivos
X fix callbacks dragenter and dragleave
X agregar varios checkeso cuando: 
  X evt.originalEvent.dataTransfer.files
  X evt.target.files
X Draggingover document, callbacks?
X methods.cancel
X methods.reset
X Problema:
  X se agregar 3 files
  X se cancela global
  X se agrega otro mas
    X el posQueue es donde se 'quedo' el queue cuando se cancelo
    X deberia ser donde se agrega el/los nuevos archivos si no hay un queueRunning
X Problema:
  X Agrego archivos al queue
  X Cancelo uno o varios o muchos
  X presiono start (global) -> se inician los cancelados: NO Deberia en aquellos cancelados by user
  X solo los failed y los pending
X al cancelar global:
  X si hay una queue runing deberia llamar la function oncomplete todos los pending
  X lo mismo con reset
X Evaular si se puede mergear cancel() y reset()
  X al menos parte del codigo duplicado que tienen
X Testing todos los modos
X Ser consistente con como se llaman los callbacks, sobretodo con los dragleave onnew file y smilares
X Code style
  X lint
  X minify
  X carpeta demos
  X OPCION multiple!
  X banner JS USING GRUNT
  X DESTROY: REWVIEW, CREAR METHOD DISTORY... QUE LLAME A RESET, Y BORRE LOS DOCUMENT DRAG LISTENERS IF DND ENABLED


X test manual queue
  X add files
  X play uno en el medio, que sucede cuando termine?

------------------------------------------------------------------------

Documentation:
  - Add to Readme.md:
      - New methods:
         * cancel
         * start
         * reset

------------------------------------------------------------------------

- Test 'devel' features: manual upload & 'auto' option.

- add 'reset' API method to reset IDs, free resources, etc. (cleanup)

- We could use onUploadProgress to also report 'time left' of the upload

- Add maxSimulaneusRequests
- Add maxFilesPerRequest

- Allow the option to do something like Facebook when draggin-in content:
   ... show/hide (we are goin to just use moar callbacks)
