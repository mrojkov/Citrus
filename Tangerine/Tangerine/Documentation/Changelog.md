##### Changelog 20.04.2009 - 25.04.2019

* Ключи анимации альфа-канала в ParticleModifier были изменены на 0 30 60 [CIT-1044](https://jira.game-forest.com:2000/browse/CIT-1044)
* Добавлена возможность копировать компоненты [CIT-1140](https://jira.game-forest.com:2000/browse/CIT-1140)
* Добавлена возможность сбрасывать анимации на начальное положение при использовании Animation Mode [CIT-1143](https://jira.game-forest.com:2000/browse/CIT-1143)
* Переделан механизм загрузки .dll OrangePlugin [CIT-1150](https://jira.game-forest.com:2000/browse/CIT-1150)
* Теперь кости игнорируются при перетаскивании виджетов на таймлайне [CIT-1157](https://jira.game-forest.com:2000/browse/CIT-1157)
* Реализован интеллектуальный автоскролл в Orange [CIT-1164](https://jira.game-forest.com:2000/browse/CIT-1164)
* Отрефакторен AssetCoocker [CIT-1165](https://jira.game-forest.com:2000/browse/CIT-1165)
* Реализован сетевой кэш для сборки билдов в Orange [CIT-1166](https://jira.game-forest.com:2000/browse/CIT-1166)
* Реализован dual quaternion skinning для 3D моделей [CIT-1170](https://jira.game-forest.com:2000/browse/CIT-1170)
* При экспорте фрейма во внешнюю сцену его значения больше не экспортируются [CIT-1180](https://jira.game-forest.com:2000/browse/CIT-1180)
* Реализованы маркеры для локалей [CIT-1192](https://jira.game-forest.com:2000/browse/CIT-1192)
* Реализованы составные анимации (CompoundAnimations)

Багфиксы:

* Исправлена проблема со "съезжающими" панелями в редакторе [CIT-922](https://jira.game-forest.com:2000/browse/CIT-922)
* Исправлена проблема, при которой использоватся по-умолчанию первый словарь в списке [CIT-1007](https://jira.game-forest.com:2000/browse/CIT-1007)
* Исправлены неработающие ограничения для значений, введенных с помощью мыши [CIT-1061](https://jira.game-forest.com:2000/browse/CIT-1061)
* Исправлена ошибка, не позволяющая назначить DefaultLayout в Layouts [CIT-1071](https://jira.game-forest.com:2000/browse/CIT-1071)
* Исправлена проблема, когда при запуске нового проекта возникал черный экран [CIT-1138](https://jira.game-forest.com:2000/browse/CIT-1138)
* Исправлена ошибка, когда Frame Progression не отрисовывается поверх всех виджетов [CIT-1167](https://jira.game-forest.com:2000/browse/CIT-1167)
* Исправлена ошибка ObjectDisposedException "Texture2D" [CIT-1174](https://jira.game-forest.com:2000/browse/CIT-1174)
* Исправлена ошибка, при которой BounceZoneThickness в ScrollView ни на что не влиял [CIT-1186](https://jira.game-forest.com:2000/browse/CIT-1186)

##### Changelog 19.03.2019

* Реализован автоскролл в Orange [CIT-1152](https://jira.game-forest.com:2000/browse/CIT-1152)
* ContentsPath теперь проверяется на наличие внешней сцены [CIT-1156](https://jira.game-forest.com:2000/browse/CIT-1156)
* Widget.Input.RestrictScope заменены Widget.Enabled в Orange [CIT-1160](https://jira.game-forest.com:2000/browse/CIT-1160)
* Реворк реализации SDF-шрифтов (SDFCleanup)

Багфиксы:

* Исправлена ошибка, при которой невозможно открыть файлы, в пути которых содержится больше одной точки [CIT-1139](https://jira.game-forest.com:2000/browse/CIT-1139)

##### Changelog 1.03.2019 - 14.03.2019

* Добавлена возможность настраивать плавность анимации через изинги (easings)
* Добавлена возможность очистить кэш, настройки и проч. прямо из редактора [CIT-874](https://jira.game-forest.com:2000/browse/CIT-874)
* Убрана возможность сделать Undo после конвертации сцены [CIT-1108](https://jira.game-forest.com:2000/browse/CIT-1108)
* Реализован прогрессбар для Orange Launcher [CIT-1144](https://jira.game-forest.com:2000/browse/CIT-1144)
* Реализован кэш для файлов, сжатых алгоритмом DDS [CIT-1146](https://jira.game-forest.com:2000/browse/CIT-1146)

Багфиксы:

* Исправлена ошибка, при которой не рендерились вложенные шрифты [CIT-1120](https://jira.game-forest.com:2000/browse/CIT-1120)
* Исправлена ошибка, при которой файлы со старшей датой изменения не перекукивались при сборке [CIT-1127](https://jira.game-forest.com:2000/browse/CIT-1127)
* Исправлена ошибка, при которой не обновлялись значения триггеров при выделении кейфрейма [CIT-1130](https://jira.game-forest.com:2000/browse/CIT-1130)

##### Changelog 1.03.2019

* Добавлена возможность сбрасывать значения свойств виджета кликом средней кнопки мыши [CIT-846](https://jira.game-forest.com:2000/browse/CIT-846)
* Реализован кэш для файлов, сжатых алгоритмом ETC [CIT-1122](https://jira.game-forest.com:2000/browse/CIT-1122)

Багфиксы:

* Исправлены проблемы с отображением меню [CIT-736](https://jira.game-forest.com:2000/browse/CIT-736)
* Исправлен некорректный сброс значений пивота [CIT-926](https://jira.game-forest.com:2000/browse/CIT-926)
* Исправлена ошибка, при которой бандл пересобирался при изменении регистра пути до файлов [CIT-1109](https://jira.game-forest.com:2000/browse/CIT-1109)
* Исправлена ошибка, при которой файлы, имеющие дату старше, чем в бандлах, не перекукивались [CIT-1127](https://jira.game-forest.com:2000/browse/CIT-1127)


##### Changelog 26.02.2019

* Добавлена возможность использовать Audio во Viewport3D [CIT-1002](https://jira.game-forest.com:2000/browse/CIT-1002)
* Добавлено отображение свойств в readonly [CIT-1096](https://jira.game-forest.com:2000/browse/CIT-1096)
* Переработана работа с кешом в режиме Animation Mode [CIT-1121](https://jira.game-forest.com:2000/browse/CIT-1121)
* Добавлена возможность обновить рабочий проект через Git напрямую через редактор [CIT-1126](https://jira.game-forest.com:2000/browse/CIT-1121)

Багфиксы:

* Исправлена ошибка, при которой выбор SplinePoint3D блокировал ввод с клавиатуры [CIT-754](https://jira.game-forest.com:2000/browse/CIT-754)
* Исправлено отображение кириллицы в окне вывода Orange [CIT-916](https://jira.game-forest.com:2000/browse/CIT-916)
* Исправлено отображение иероглифов для различных языков в свойстве Text [CIT-1015](https://jira.game-forest.com:2000/browse/CIT-1015)
* Исправлена ошибка, приводящая к поломке сцен с Viewport3D при конвертации из .scene в .tan [CIT-1082](https://jira.game-forest.com:2000/browse/CIT-1082)
* Исправлена ошибка, приводящая к крашу при использовании унарных тегов [CIT-1133](https://jira.game-forest.com:2000/browse/CIT-1133)


##### Changelog 22.02.2019

Багфиксы:

* Исправлена ошибка, не позволяющая скроллить вывод Orange [CIT-220](https://jira.game-forest.com:2000/browse/CIT-220)
* Увеличена производительность при работе с Model3D [CIT-974](https://jira.game-forest.com:2000/browse/CIT-974)
* Исправлена ошибка, не обновляющая значение триггеров при выборе фреймов [CIT-1130](https://jira.game-forest.com:2000/browse/CIT-1130)

##### Changelog 20.02.2019

* Переделан интерфейс работы с триггерами [CIT-1062](https://jira.game-forest.com:2000/browse/CIT-1062), [CIT-1112](https://jira.game-forest.com:2000/browse/CIT-1112)

Багфиксы:

* Исправлена ошибка, когда из параллельной анимации вызывались неиспользуемые триггеры [CIT-1118](https://jira.game-forest.com:2000/browse/CIT-1118)
* Исправлена ошибка, при которой вызов "Update XCode Project" пересобирал все доступные бандлы [CIT-1125](https://jira.game-forest.com:2000/browse/CIT-1125)

##### Changelog 15.02.2019

* Реализована возможность объединять меши с одинаковыми материалами в Model3D [CIT-1115](https://jira.game-forest.com:2000/browse/CIT-1115)

Багфиксы:

* Исправлена ошибка, приводящая к крашу редактору при использовании некоторых мешей в .fbx [CIT-1123](https://jira.game-forest.com:2000/browse/CIT-1123)
* Исправлены другие мелкие ошибки и проблемы в производительности.

##### Changelog 13.02.2019

* Возвращена возможность таскать ключи анимации без клика на фрейме [CIT-1119](https://jira.game-forest.com:2000/browse/CIT-1119)

Багфиксы:

* Исправлены ошибки и недоработки, связанные с работой Hierarchy [CIT-866](https://jira.game-forest.com:2000/browse/CIT-866), [CIT-1008](https://jira.game-forest.com:2000/browse/CIT-1008)
* Исправлены ошибки, связанные с LetterSpacing в RichText.

##### Changelog 12.02.2019

* Добавлена возможность рендерить анимацию в последовательность PNG-файлов [CIT-1009](https://jira.game-forest.com:2000/browse/CIT-1009)
* Доработан функционал `Lock timeline cursor` [CIT-1110](https://jira.game-forest.com:2000/browse/CIT-1110)

Багфиксы:

* Исправлена ошибка, при которой настройки шейдеров/блендинга не наследовались в DistortionMesh [CIT-1102](https://jira.game-forest.com:2000/browse/CIT-1102)

##### Changelog 11.02.2019

* `Lock widgets` теперь блокирует виджет от редактирования [CIT-1095](https://jira.game-forest.com:2000/browse/CIT-1095)
* Добавлены аниматоры для NumericRange [CIT-1114](https://jira.game-forest.com:2000/browse/CIT-1114)

##### Changelog 07.02.2019

Багфиксы:

* Исправлены проблемы, связанные с конвертацией .scene в .tan [CIT-964](https://jira.game-forest.com:2000/browse/CIT-964), [CIT-1080](https://jira.game-forest.com:2000/browse/CIT-1080)
* Исправлена ошибка, приводящая к остановке анимации при использовании Animation Mode [CIT-1048](https://jira.game-forest.com:2000/browse/CIT-1048)

##### Changelog 06.02.2019

* Добавлен выбор анимации по-умолчанию для 3d-моделей в 3DAttachment [CIT-1099](https://jira.game-forest.com:2000/browse/CIT-1099)

Багфиксы:

* Исправлен краш при открытии Model3D в сцене через панель Hierarchy [CIT-1100](https://jira.game-forest.com:2000/browse/CIT-1100)
* В root-сцене убрана возможность менять Position [CIT-1092](https://jira.game-forest.com:2000/browse/CIT-1092)

##### Changelog 05.02.2019

* При использовании TiledImage выдаётся предупреждение, если задействованная текстура не имеет `wrap mode: repeat` [CIT-779](https://jira.game-forest.com:2000/browse/CIT-779)
* Реализована возможность конвертировать виджет во внешнюю сцену через контекстное меню в списке таймлайна [CIT-1049](https://jira.game-forest.com:2000/browse/CIT-1049)
* Добавлена возможность игнорировать подгружающиеся типы в `dropdown list` [CIT-1078](https://jira.game-forest.com:2000/browse/CIT-1078)
* Добавлена возможность уменьшать список виджетов таймлайна в ширину [CIT-1094](https://jira.game-forest.com:2000/browse/CIT-1094)
* Добавлена возможность фиксировать каретку на таймлайне [CIT-1097](https://jira.game-forest.com:2000/browse/CIT-1097)

Багфиксы:

* Исправлена ошибка, сбрасывающая значения альфа-канала при использовании инструмента "пипетка" [CIT-1001](https://jira.game-forest.com:2000/browse/CIT-1001)
* Исправлена ошибка, при которой виртуальная клавиатура перестается отвечать на ввод [CIT-1057](https://jira.game-forest.com:2000/browse/CIT-1057)
* Исправлено неправильное отображение кнопки удаления анимаций [CIT-1058](https://jira.game-forest.com:2000/browse/CIT-1058)
* Исправлена некорректная работа LetterSpacing у TextStyle [CIT-1063](https://jira.game-forest.com:2000/browse/CIT-1063)
* Исправлен краш при выборе `ImageUsage -> Overlay` в TextStyle [CIT-1076](https://jira.game-forest.com:2000/browse/CIT-1076)
* Исправлены различные краши при использовании Gradient Component [CIT-1083](https://jira.game-forest.com:2000/browse/CIT-1083), [CIT-1084](https://jira.game-forest.com:2000/browse/CIT-1084)
* Цвета теперь корректно свапаются при перестановке точек местами в Gradient Component [CIT-1085](https://jira.game-forest.com:2000/browse/CIT-1085)
* Исправлен краш при выборе инструмента "пипетка" при отсутствии открытого проекта [CIT-1088](https://jira.game-forest.com:2000/browse/CIT-1088)
* Исправлена ошибка, позволяющая сбросить развёрнутое на весь экран окно до исходного размера [CIT-1089](https://jira.game-forest.com:2000/browse/CIT-1089)
* Исправлена ошибка, позволяющая отрисовывать несколько Animation Path одновременно при использовании параллельных анимаций [CIT-1093](https://jira.game-forest.com:2000/browse/CIT-1093)

##### Changelog 04.02.2019

* Реализовано визуальное отображение изменения виджета на определенном промежутке времени (Frame Progression) [CIT-721](https://jira.game-forest.com:2000/browse/CIT-721)

##### Changelog 22.01.2019

Багфиксы:

* Исправлена ошибка, приводящая к крашу при выборе списка из выпадающего списка листов [CIT-1081](https://jira.game-forest.com:2000/browse/CIT-1081)

##### Changelog 11.01.2019

Багфиксы:

* Исправлена ошибка, позволяющая циклические зависимости между сценами [CIT-835](https://jira.game-forest.com:2000/browse/CIT-835)
* Исправлена ошибка, делающая невозможным изменение цвета во множественном выделении [CIT-1035](https://jira.game-forest.com:2000/browse/CIT-1035)
* Исправлена ошибка, позволяющая заменить содержимое root-сцены [CIT-1040](https://jira.game-forest.com:2000/browse/CIT-1040)
* Исправлена ошибка, делающая невозможным обновление проекта XCode [CIT-1055](https://jira.game-forest.com:2000/browse/CIT-1055)

##### Changelog 29.12.2018

* При клике на надпись `<many values>` она автоматически убирается [CIT-1021](https://jira.game-forest.com:2000/browse/CIT-1021)

Багфиксы:

* Исправлена ошибка, не позволяющая изменять значение Color у нескольких выбранных виджетов, если они имели различные значения данного параметра [CIT-1022](https://jira.game-forest.com:2000/browse/CIT-1022)

##### Changelog 26.12.2018

* Реализована возможность отображать предупреждения и ошибки прямо в панели Inspector [CIT-1016](https://jira.game-forest.com:2000/browse/CIT-1016)

Багфиксы:

* Исправлена ошибка, позволяющая случайно зайти в контейнер при изменении значения в инспекторе [CIT-783](https://jira.game-forest.com:2000/browse/CIT-783)
* Исправлена ошибка, приводящая к крашу, если введенное значение Color было больше 4 байт [CIT-1034](https://jira.game-forest.com:2000/browse/CIT-1034)

##### Changelog 19.12.2018

* Добавлен новый тип блендинга PremultipliedAlpha для корректной работы RenderTarget с прозрачностью [CIT-971](https://jira.game-forest.com:2000/browse/CIT-971)
* Исправлена ошибка, приводящая к невозможности настроить видимость виджета после взаимодействия с Hierarchy [CIT-973](https://jira.game-forest.com:2000/browse/CIT-973)
* Исправлена некорректная работа LetterSpacing в Text [CIT-1014](https://jira.game-forest.com:2000/browse/CIT-1014)

##### Changelog 18.12.2018

* Реализована возможность анимировать кастомные enum-поля [CIT-995](https://jira.game-forest.com:2000/browse/CIT-995)
* Параметры Position, Size, и Rotation теперь по-умолчанию изменяются с шагом в 1 единицу [CIT-582](https://jira.game-forest.com:2000/browse/CIT-582)

##### Changelog 14.12.2018

* Добавлена возможность свернуть список аниматоров виджета, выбрав один из аниматоров [CIT-827](https://jira.game-forest.com:2000/browse/CIT-827)
* Реализована валидация текстовый полей окна Inspector [CIT-932](https://jira.game-forest.com:2000/browse/CIT-932)

Багфиксы:

* Исправлена ошибка, позволяющая переместить таймлайн кликом с зажатым Ctrl + Shift [CIT-931](https://jira.game-forest.com:2000/browse/CIT-931)
* Исправлена ошибка при копировании аниматоров виджетов с расвёрнутым списком аниматоров [CIT-933](https://jira.game-forest.com:2000/browse/CIT-933)
* Исправлена ошибка, ломающая сцену, если в ContentsPath Viewport3D вставить внешнюю сцену с отличным от Viewport3D root-сценой [CIT-993](https://jira.game-forest.com:2000/browse/CIT-993)

##### Changelog 12.12.2018

* Исправлена ошибка, позволяющая удалять ключи анимаций глобально из всех анимаций [CIT-996](https://jira.game-forest.com:2000/browse/CIT-996)

##### Changelog 11.12.2018

* Для кнопок в играх и редакторе теперь можно задавать доп. лимиты, где кнопка считается нажатой [CIT-902](https://jira.game-forest.com:2000/browse/CIT-902)
* Реализована возможность использовать SDF-шрифты в RichText и SimpleText [CIT-929](https://jira.game-forest.com:2000/browse/CIT-929)

Багфиксы:

* Исправлена ошибка, приводящая к невозможности снять выделение с отдельного фрейма, используя Ctrl [CIT-967](https://jira.game-forest.com:2000/browse/CIT-CIT-967)


##### Changelog 07.12.2018

* Исправлена ошибка, приводящая к нерабочим анимациям, если в Model3D Attachment название анимации выставлено в отличное от Default значение

`commit: a0d0a7cf79f490e8a2f3e5159634c5420d72aaa4`

##### Changelog 06.12.2018

* Исправлена ошибка, приводящая к необновляемым значениями в инспекторе [CIT-973](https://jira.game-forest.com:2000/browse/CIT-973)

##### Changelog 05.12.2018

* Увеличена производительность редактора при работе со внешними сценами [CIT-428](https://jira.game-forest.com:2000/browse/CIT-428)
* Внешние сцены больше не подгружаются, если не были изменены [CIT-719](https://jira.game-forest.com:2000/browse/CIT-719)
* Добавлена возможность переназначать материал в Model3D Attachment [CIT-912](https://jira.game-forest.com:2000/browse/CIT-912)
* Переделана вкладка "Mesh Options" в Model3D Attachment [CIT-913](https://jira.game-forest.com:2000/browse/CIT-913)
* AlignmentPropertyEditor теперь используется для Alignment [CIT-919](https://jira.game-forest.com:2000/browse/CIT-919)
* В объектах типa Bone (кости) теперь доступны параметры Index и BaseIndex (в readonly) [CIT-975](https://jira.game-forest.com:2000/browse/CIT-975)

Багфиксы:

* Исправлена ошибка, при которой открывался последний закрытый проект после перезапуска редактора [CIT-839](https://jira.game-forest.com:2000/browse/CIT-839)
* Исправлена ошибка, приводящая к крашу при попытке дублирования (`Ctrl + D`) виджета ImageCombiner и свёрнутой папки (Folder) [CIT-956](https://jira.game-forest.com:2000/browse/CIT-956)
* Исправлена ошибка, приводящая к неправильной работе Undo после удаления столбца [CIT-968](https://jira.game-forest.com:2000/browse/CIT-968)

##### Changelog 30.11.2018

* Исправлено медленное удаление столбца на таймлайне (через `Ctrl + W`) [CIT-739](https://jira.game-forest.com:2000/browse/CIT-739)
* Исправлено поведение Resize, если положение пивота виджета было отлично от значений по-умолчанию [CIT-800](https://jira.game-forest.com:2000/browse/CIT-800)
* При множественном выделении теперь отображается `<many values>`, если значения у свойств виджетов было разное [CIT-884](https://jira.game-forest.com:2000/browse/CIT-884)
* В исключения валидатора FilesystemPropertyEditor добавлен символ "-" [CIT-910](https://jira.game-forest.com:2000/browse/CIT-910)
* При смене виджета, если скролл оказался в недоступной зоне, он откатывается на последнюю доступную позицию [CIT-920](https://jira.game-forest.com:2000/browse/CIT-920)
* Исправлена ошибка, не позволяющая сохранять текущий тип интерполяции на таймлайне при работе с кейфреймами [CIT-935](https://jira.game-forest.com:2000/browse/CIT-935)
* RowCount и ColumnCount в Table Layout больше не могут быть отрицательными значениями [CIT-945](https://jira.game-forest.com:2000/browse/CIT-945)
* Исправлена проблема, приводящая к ошибки в обновлении дерева Hierarchy [CIT-957](https://jira.game-forest.com:2000/browse/CIT-957)

##### Changelog 26.11.2018

* Исправлена ошибка, приводящая к крашу, если была использована модель .fbx c частотой кадров 30 fps [CIT-947](https://jira.game-forest.com:2000/browse/CIT-947)

##### Changelog 22.11.2018

* Исправлена ошибка в SplineGear, приводящая к неверному просчету анимации при использовании AlongPathOrientation [CIT-905](https://jira.game-forest.com:2000/browse/CIT-905)
* Исправлена ошибка, позволяющая обходить валидацию имени виджетов через переименование в списке Timeline [CIT-906](https://jira.game-forest.com:2000/browse/CIT-906)
* Исправлен краш при установке ключа анимации хоткеем при развернутом списке аниматоров [CIT-918](https://jira.game-forest.com:2000/browse/CIT-918)
* Исправлена ошибка, приводящая к применению всех доступных анимаций родительского виджета к дочерним, когда этого не требовалось [CIT-925](https://jira.game-forest.com:2000/browse/CIT-925)

##### Changelog 15.11.2018

* Теперь позиция скролла в Inspector сохраняется при выборе однотипного виджета [CIT-882](https://jira.game-forest.com:2000/browse/CIT-882)
* Реализован проброс параметра `tag` в `TaskList.AddLoop` [CIT-904](https://jira.game-forest.com:2000/browse/CIT-904)

Багфиксы:

* Исправлена ошибка, позволяющая создать виджет с отрицательными значениями параметра Size [CIT-896](https://jira.game-forest.com:2000/browse/CIT-896)
* Исправлена ошибка, не позволяющая возвращать исходные значения параметра (после Undo), если они были изменены через Paste Keyframes [CIT-903](https://jira.game-forest.com:2000/browse/CIT-903)

##### Changelog 14.11.2018
* Добавлена документация по редактору. Теперь узнать информацию по Tangerine можно, вызвав Help -> View Help `Ctrl + F1` (на данный момент работает только на платформе Win) [CIT-798](https://jira.game-forest.com:2000/browse/CIT-798)

Багфиксы:

* Исправлена ошибка, создающая в дочерних виджетах лейаутов LayoutConstraints, если у виджета уже был добавлен кастомный компонент, наследуемый от класса LayoutConstraints [CIT-752](https://jira.game-forest.com:2000/browse/CIT-752)
* Исправлена ошибка, приводящая к невозможности смены типа интерполяции у множественного выделения виджетов [CIT-848](https://jira.game-forest.com:2000/browse/CIT-848)
* Исправлена регрессия, не позволяющая скроллить список виджетов таймлайна при перетаскивании виджетов [CIT-881](https://jira.game-forest.com:2000/browse/CIT-881)

##### Changelog (12.11.2018 - 13.11.2018)

* Разворачиваемые списки больше не сворачиваются при смене кадра [CIT-764](https://jira.game-forest.com:2000/browse/CIT-764)
* Теперь по клику в любое доступное место можно снять фокус с текущего активного текстового/числового поля [CIT-792](https://jira.game-forest.com:2000/browse/CIT-792)
* MFDecoder убран из движка в 3rdparty [CIT-879](https://jira.game-forest.com:2000/browse/CIT-879)

##### Changelog (01.11.2018 - 9.11.2018)

* Переработан интерфейс работы с Attachment3D. Теперь он вызывается как панель (View -> Panels -> Model3D Attachment) [CIT-744](https://jira.game-forest.com:2000/browse/CIT-744)
* Добавлен выбор источника анимации при использовании Model3D.
По-умолчанию теперь будут импортироваться все анимации из fbx модели (раньше импортировалась только одна). Следовательно, можно создавать разные анимации вместо размещения всех анимаций на одном таймлайне. В 3DAttachment можно явно указать, какую анимацию нужно перегрузить [CIT-795](https://jira.game-forest.com:2000/browse/CIT-795)

Багфиксы:

* Исправлена ошибка, позволяющая при анимировании поля Text создавать новые ключи анимации без Automatic Keyframes [CIT-763](https://jira.game-forest.com:2000/browse/CIT-763)
* Исправлена ошибка, приводящая к крашу, если применить Fit to Container при выставленном ключе анимации Rotation [CIT-786](https://jira.game-forest.com:2000/browse/CIT-786)
* Исправлена ошибка, приводящая к невозможности использовать стандартные операции (вставка/копирование/вырезание и т.п.) над аниматорами кастомных виджетов [CIT-864](https://jira.game-forest.com:2000/browse/CIT-864) / [CIT-887](https://jira.game-forest.com:2000/browse/CIT-887)
* Исправлена ошибка, приводившая к крашу при копировании виджета с анимированным Spacing в любом Layout [CIT-886](https://jira.game-forest.com:2000/browse/CIT-886)
* Исправлена ошибка, приводивщая к пробрасыванию нажатия Enter в диалоге создания маркера [CIT-891](https://jira.game-forest.com:2000/browse/CIT-891)

##### Changelog (1.10.2018 - 1.11.2018)
* Ревамп внешнего вида редактора, изменен дизайн табов панелей и толщина сплиттеров между ними [CIT-332](https://jira.game-forest.com:2000/browse/CIT-332)
* Была изменена логика работы с пивотами - теперь активный (выбранный) пивот всегда отображается поверх остальных, также добавлена цветовая индикация. [CIT-810](https://jira.game-forest.com:2000/browse/CIT-810)
* Убрана задержка при драге виджетов (теперь она справедлива только для дублирования через Alt) [CIT-819](https://jira.game-forest.com:2000/browse/CIT-819)
* Меню, которые невозможно использовать в данный момент, теперь остаются неактивными [CIT-850](https://jira.game-forest.com:2000/browse/CIT-850)
* Начат переход к использованию параллельных анимаций. Подробная информация об этом и как это использовать будет позднее. Кратко: теперь есть возможность создавать несколько неймспейсов у корневых виджетов (контейнеров), в пределах этих неймспейсов будут свои ключи анимации и маркеры. Применятся они, соответственно, будут на дочерние контейнеру виджеты
* Исправлено множество ошибок и багов. В том числе устранена проблема с некорректной обработкой даблкликов и большинство проблем с инпутом

##### Changelog (11.09.2018 - 1.10.2018)
* Теперь Orange Launcher регенерирует бинарные десериалайзеры Yuzu [CIT-110](https://jira.game-forest.com:2000/browse/CIT-110)
* Добавлена обводка для маркеров на таймлайне [CIT-713](https://jira.game-forest.com:2000/browse/CIT-713)
* Добавлена возможность работы с аниматорами на таймлайне [CIT-720](https://jira.game-forest.com:2000/browse/CIT-720)
* Реализован ListPropertyEditor (редактор полей типа List<T>) [CIT-750](https://jira.game-forest.com:2000/browse/CIT-750)
* Для текста теперь можно указать PathPropertyEditor [CIT-765](https://jira.game-forest.com:2000/browse/CIT-765)
* Недостающие цвета в конфиге теперь берутся из текущей темы [CIT-766](https://jira.game-forest.com:2000/browse/CIT-766)
* При запуске редактора неиспользуемые сцены не грузятся до их открытия [CIT-790](https://jira.game-forest.com:2000/browse/CIT-790)
* Изменена индикация изменений на сцене [CIT-796](https://jira.game-forest.com:2000/browse/CIT-796)
* Исправлено множество багов и недоработок

##### Changelog (1.08.2018 - 11.09.2018)
* Добавлен пользовательский компонент Gradient [CIT-102](https://jira.game-forest.com:2000/browse/CIT-102)
* Добавлен пользовательский компонент HSL [CIT-650](https://jira.game-forest.com:2000/browse/CIT-650)
* Добавлен пользовательский компонент Layout - Stack, Table, Flow, Linear. [layout branch report](https://jira.game-forest.com:8888/snippets/13)
* Добавлен фильтр элементов для Viewport (Visual Hints) [CIT-246](https://jira.game-forest.com:2000/browse/CIT-246)
* Добавлена индикация связей костей/SplineGear с виджетами [CIT-267](https://jira.game-forest.com:2000/browse/CIT-267), [CIT-612](https://jira.game-forest.com:2000/browse/CIT-612)
* Добавлена возможность раскрывать список аниматоров хоткеем (`Shift + Space`) [CIT-274](https://jira.game-forest.com:2000/browse/CIT-274)
* Добавлена возможность выставить/убрать ключи анимации хоткеями (Position, Scale, Rotation - `E/R/T`) [CIT-280](https://jira.game-forest.com:2000/browse/CIT-280), [CIT-673](https://jira.game-forest.com:2000/browse/CIT-673)
* Добавлена возможность Copy/Paste в курсор мыши [CIT-286](https://jira.game-forest.com:2000/browse/CIT-286)
* Добавлена возможность производить поиск по полю Text [CIT-344](https://jira.game-forest.com:2000/browse/CIT-344)
* Добавлен общий заголовок для панелей [CIT-349](https://jira.game-forest.com:2000/browse/CIT-349)
* Добавлена возможность конвертировать Frame в Button [CIT-379](https://jira.game-forest.com:2000/browse/CIT-379)
* Переработана работа с цветовыми схемами в редакторе [CIT-417](https://jira.game-forest.com:2000/browse/CIT-417)
* Добавлена поддержка арифметический операций в числовых полях [CIT-431](https://jira.game-forest.com:2000/browse/CIT-431)
* Добавлена возможность смещать/масштабировать ключи анимации (Numeric Scale/Move Keys) [CIT-450](https://jira.game-forest.com:2000/browse/CIT-450)
* Исправлено отображение мелких записей в интерфейсе [CIT-466](https://jira.game-forest.com:2000/browse/CIT-466)
* Добавлена возможность центрировать экран на выбранном виджете/ноде (Center View) [CIT-490](https://jira.game-forest.com:2000/browse/CIT-490)
* Превью виджетов по `Tab` теперь работает циклически [CIT-495](https://jira.game-forest.com:2000/browse/CIT-495)
* Окна панелей теперь имеют класс ToolWindow [CIT-496](https://jira.game-forest.com:2000/browse/CIT-496)
* Добавлен механизм проверки "плохих" путей файлов [CIT-500](https://jira.game-forest.com:2000/browse/CIT-500)
* Переработана панель инструментов (теперь ее можно кастомизировать) [CIT-501](https://jira.game-forest.com:2000/browse/CIT-501)
* Добавлена возможность быстрого масштабирования ключей анимации на таймлайне (растягиванием выделения) [CIT-525](https://jira.game-forest.com:2000/browse/CIT-525)
* Переработана панель Filesystem с поддержкой адресной строки и навигации по клавишам (как в Windows Explorer) [CIT-534](https://jira.game-forest.com:2000/browse/CIT-534)
* Добавлена возможность центрироваться на каретке по хоткею (`Ctrl + Shift + C`) [CIT-539](https://jira.game-forest.com:2000/browse/CIT-539)
* Добавлено превью в Filesystem для .tan [CIT-556](https://jira.game-forest.com:2000/browse/CIT-556)
* Ускорена работа с ключами анимации/маркерами на таймлайне [CIT-557](https://jira.game-forest.com:2000/browse/CIT-557)
* Реализован новый виджет - TiledImage [CIT-560](https://jira.game-forest.com:2000/browse/CIT-560)
* Добавлена возможность drag & drop для TiledImage [CIT-625](https://jira.game-forest.com:2000/browse/CIT-625)
* Добавлена возможность выставить свой коэф. Zoom [CIT-581](https://jira.game-forest.com:2000/browse/CIT-581)
* Добавлена возможность изменения рамок виджета без затрагивания его содержимого [CIT-589](https://jira.game-forest.com:2000/browse/CIT-589)
* Реализован AlongPathOrientation для SplineGear [CIT-607](https://jira.game-forest.com:2000/browse/CIT-607)
* Переработана панель Search - переименована в Hierarchy, реализовано в виде древовидной структуры [CIT-619](https://jira.game-forest.com:2000/browse/CIT-619)
* Удалена возможность анимировать Root-cцену [CIT-613](https://jira.game-forest.com:2000/browse/CIT-613)
* Добавлен режим Slowmotion (x0.10) для режима проигрывания анимации (F5) [CIT-629](https://jira.game-forest.com:2000/browse/CIT-629)
* Реализован базовый функционал внутренней справки [CIT-634](https://jira.game-forest.com:2000/browse/CIT-634)
* Переработан редактор текста для RichText/SimpleText [CIT-641](https://jira.game-forest.com:2000/browse/CIT-641)
* Переработан внешний вид меню (добавлены иконки и сплиттеры) [CIT-662](https://jira.game-forest.com:2000/browse/CIT-662)
* Unsample animation twice теперь работает только внутри контейнера [CIT-663](https://jira.game-forest.com:2000/browse/CIT-663)
* Добавлена возможность выбрать все ключи анимации для текущей строки (`Ctrl + Shift + A`) и выбрать только ключи анимации выделением через `Alt ` [CIT-668](https://jira.game-forest.com:2000/browse/CIT-668)
* Выделение теперь сбрасывается по клику на выделенной области [CIT-670](https://jira.game-forest.com:2000/browse/CIT-670)
* Изменен порядок интерполяций по-умолчанию (Linear → Spline → Steep → ClosedSpline) [CIT-671](https://jira.game-forest.com:2000/browse/CIT-671)
* Добавлена возможность скрывать только выделенные виджеты на таймлайне (Shift + Show widgets) [CIT-675](https://jira.game-forest.com:2000/browse/CIT-675)
* Реализована возможность добавить кастомные компоненты в Model3DAttachment [CIT-697](https://jira.game-forest.com:2000/browse/CIT-697)
* Добавлена возможность изменять префикс во множественном выделении виджетов [CIT-699](https://jira.game-forest.com:2000/browse/CIT-699)
* Реализована функция Timeshift для ParticleEmitter (аналогично HotStudio) [CIT-708](https://jira.game-forest.com:2000/browse/CIT-708 )
* Исправлено множество багов и недоработок

##### Changelog (11.08.2018 - 1.10.2018)
* Теперь Orange Launcher регенерирует бинарные десериалайзеры Yuzu [CIT-110](https://jira.game-forest.com:2000/browse/CIT-110)
* Добавлена обводка для маркеров на таймлайне [CIT-713](https://jira.game-forest.com:2000/browse/CIT-713)
* Добавлена возможность работы с аниматорами на таймлайне [CIT-720](https://jira.game-forest.com:2000/browse/CIT-720)
* Реализован ListPropertyEditor (редактор полей типа List<T>) [CIT-750](https://jira.game-forest.com:2000/browse/CIT-750)
* Для текста теперь можно указать PathPropertyEditor [CIT-765](https://jira.game-forest.com:2000/browse/CIT-765)
* Недостающие цвета в конфиге теперь берутся из текущей темы [CIT-766](https://jira.game-forest.com:2000/browse/CIT-766)
* При запуске редактора неиспользуемые сцены не грузятся до их открытия [CIT-790](https://jira.game-forest.com:2000/browse/CIT-790)
* Изменена индикация изменений на сцене [CIT-796](https://jira.game-forest.com:2000/browse/CIT-796)
* Исправлено множество багов и недоработок
