   M S B A M L   `   `   `  ����G  CInvoicingApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:InvoicingAppInvoicingApp  5   	   !    InvoicingApp.UpdateCustomer   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:InvoicingApp     5   	   -   5          � Title$  Update Customer��5   	   $	��225��6!   $	��370��6.   .��5      �� .+�5   
   �� $��Select Customer��6    �� Left$ 10��6*   
 �� Top$ 10��6;   �� 5   
   $
��Name��6   $ 10��6   $ 40��60   �� 5   
   $��Street Address��6   $ 10��6)   $ 65��6:   �� 5   
   $��Postal Code��6   $ 10��6&   $ 90��67   �� 5   
   $
��City��6   $ 10��6   $	 115��60   �� 5   
   -   5        $��CustomerComboBox��5      $	 115��5      $ 12��5      $��25��5      $	��200��5      ��5      �� *	Customers+ �� SelectedItem 5      �� *SelectedCustomer+��5      �� 5      .R�5      ��  �� Orientation$ 
Horizontal=�6!   .+�5      �� ��6$   �� *
CustomerId+ �� StringFormat$ ID {0}: ���� 5      ��6$   �� *Name+5       5!      5#      �� 5%   
   -   5        $��NameBox��5%      $	 115��5&      $ 45��5'      $	��200��6   ��5(      �� *SelectedCustomer.Name+�� 5*   
   -   5        $��StreetAddressBox��5*      $	 115��5+      $ 70��5,      $	��200��6   ��5-      �� * SelectedCustomer.StreetAddress+�� 5/   
   -   5        $��PostalCodeBox��5/      $	 115��50      $ 95��51      $	��200��6   ��52      �� *SelectedCustomer.PostalCode+�� 54   
   -   5        $��CityBox��54      $	 115��55      $	 120��56      $	��200��6#   ��57      �� *SelectedCustomer.City+�� 59   
   -   5        $
��Save��59      $	 200��6    $	 145��62   $��55��6C   6N   �� 5:   
   -   5        $��Cancel��5:      $	 260��6"   $	 145��64   $��55��6E   6P   5;      5<      