   M S B A M L   `   `   `  ����   PuzzleBobble    PuzzleBobble.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Bust-A-Move! EWU Edition��$	��510��$	��525��.��� -    �Name$	 grd��.+��� -    ��Name$ canvasPlayArea��$
��Left=�$	��440��$��	10,28,0,0q�$	��Top=�$	��300��}�. ����  �� ImageSource$ /Images/grid.gif�� j� TileMode$
 Tile=� j� ViewportUnits$ Absolute=� j� Viewport$ 	0,0,60,60�.+��� -    ��Name$ overflowBar��$��/Images/bar.png��$	��300��$��6��
 �� Top$	 364���� -   	 ��Name$	 leftWall��$
��Left=�$��36��$	��440��$��0,0,0,0q�
��������� -   $	 	rightWall��$
��Left=�$��36��$	��440��$��	264,0,0,0q�
��������� -   $	 ceiling��$
��Left=�$	��230��$	��360��$��0,0,0,0q�
 �� Left$
 35��$
 -332������ $ /Images/ceiling.png���� $
��Left=�$	��230��$��28��$
 35��
��������� -   $ gamePointer��$	��110��$	
 136��$	 320��$��28�� J� RenderTransformOrigin$ 	0.5,0.565&��� -	   $	 nextBox��$��28��$
 70��$	 402��$��28���� $��Next:��$
 65��$	 375��
��������� $��/Images/wall.png��$	��440��$	��300��$
 -2���� $��Score:��$
��Left=�$��
315,28,0,0q�$	��Top=�$��16(��� -
    ��Name$ 
labelScore��$��0��$
��Left=�$��
315,52,0,0q�$	��Top=�$��20(��� -   $ canvasBubbleTest��$
��Left=�$��28��$��
414,52,0,0q�$	��Top=�$��28��o� $��Stretch=�$	��Top=�.@�m� $��_System��.@�m� -    m�Name$ menuItemNewGame��$��	_New Game��m� -   $ menuItemExit��$��E_xit��m� $��_Help��.@�m� -   $ menuItemRules��$��How to _Play��m� -   $ menuItemAbout��$��_About���� -   $ labelCeilDrop��$��Ceiling Dropping In:��$
��Left=�$��315,116,0,0q�$	��Top=�$��16(�$y�Hidden=��� -   $ labelCeilingCounter��$�� ��$
��Left=�$��315,148,0,0q�$	��Top=�$��26��$��20(��� -   $ labelGameOver��$��	Game Over��$
��Left=�$��351,378,0,0q�$	��Top=�$��24(�$y�Hidden=��� -    ��Name$ textBlockPlayAgain��$
��Left=�$��334,420,0,0q�$
��Wrap=�$+��%Go to System > New Game to Play Again��$	��Top=�$��36��$	��171�� �� TextAlignment$ Center=�$y�Hidden=�