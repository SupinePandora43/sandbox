xcopy /E /Y entity\ ..\..\..\entity\
xcopy /E /Y materials\ ..\..\..\materials\
xcopy /E /Y models\ ..\..\..\models\
xcopy /E /Y particles\ ..\..\..\particles\
xcopy /E /Y sounds\ ..\..\..\sounds\
rmdir /q /s ..\..\..\code\ui\sandbox\
xcopy /E /Y code\ui\sandbox\ ..\..\..\code\ui\sandbox\
