   M S B A M L   `   `   `  ����[  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms  5   	    CoordinateMappingBasics-WPFA   ;Microsoft.Samples.Kinect.CoordinateMappingBasics.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     5   	   -   5          � Title$  Coordinate Mapping Basics��5   	   $	��600��5   	   $	��700��6    � WindowStyle$
 None=�5   &    � WindowState$ 	Maximized=�69    � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  ScreenshotButton �� 
TargetType	 �� Orientation
 ��Name �� TextAlignment ��Name �� Triggers P� Property P� Value �� 
TargetName��%a  &	        &	 #     &	 F     '�� i     &	 �     7  7  �� 5	   
   $ 	#ff6e6e6e��62   �� 5
   
   $ 	#ff52318f��64   �� 5   
   $ 	#ff00BCF2��62   �� 5   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� $	 
Horizontal=�6)   
������ .+�5   "   �� $
 ScreenshotText��6,   #��z��6D   $
 Left=�6e   $��Center=�6{   8��  $��15(�6�   � 5   "   $��9,0,0,0q�6'   .+�5   &   �� $ ScreenshotNormal��6,   $!��Images\ScreenshotNormal.png��6F   $
��None=�6k   $��Center=�6z   �� 5   &   $ ScreenshotHover��6,   $ ��Images\ScreenshotHover.png��6E   $
��None=�6i   $��Center=�6x   $y�	Collapsed=�6�   5   #   5      5       5      P� 5       �@J�IsMouseOver . . �5   "   �� $ ScreenshotNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5   "   $ ScreenshotHover��6O    � y�$ Visible=�6?   6O   �� 5    "   8  $ ScreenshotText��6h    � ��5!      5#      5%      5&      5'      .��5(      � $��	10 0 10 0q�6    � RowDefinitions 5)   
   �� 5*      $
��Auto �6   �� 5+      $��* �6   �� 5,      $
��Auto �6   �� 5-      $
��Auto �6   .+�54   
   (� $��1��6   $��Center=�6   .��55      � -   5        $��CompositeImage��55      .+�56      �� -   5        $��
background��56      $]��Whttps://s-media-cache-ak0.pinimg.com/736x/26/a7/0b/26a70bc6ab2272b1cdd8475ab3a54fb0.jpg��6*   $��UniformToFill=�6�   �� 57      $��UniformToFill=�67   ��6   �� *ImageSource+67   5;      5<      5D      5E      