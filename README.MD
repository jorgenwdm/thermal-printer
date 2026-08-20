/*
Incase you need permissions for usb device reading:

1. Create the file: 
   
   /etc/udev/rules.d/99-usb-printer.rules:

2. Edit its content:

   # Permissions 0666 for all USB printers
   SUBSYSTEM=="usb", ATTRS{bDeviceClass}=="07", MODE="0666", GROUP="plugdev"

   # Permissions 0666 for a specific USB printer (incase it does not belong to class 07)
   # SUBSYSTEM=="usb", ATTRS{idVendor}=="0416", ATTRS{idProduct}=="5011", MODE="0666", GROUP="plugdev"

3. Add your user to the group "plugdev":

   sudo usermod -aG plugdev $USER

4. Restart udev rules:
   
   sudo udevadm control --reload-rules && sudo udevadm trigger

5. Logout and back in

*/


// For Microsoft Windows environments, you should install
/*
Συνδέεις τον εκτυπωτή, βλέπεις VID/PID στο Device Manager.
Τρέχεις το Zadig (https://zadig.akeo.ie) — GUI εργαλείο ακριβώς για αυτό.
Επιλέγεις τη συσκευή (ή το συγκεκριμένο interface της, αν έχει πολλαπλά) και αντικαθιστάς τον driver με WinUSB (προτεινόμενο, πιο σύγχρονο) ή libusbK.
Μετά, το LibUsbDotNet τη βλέπει κανονικά σαν raw USB device.
*/

// For Ubuntu Linux environments, you should install
// sudo apt update
// sudo apt install libusb-1.0-0-dev

/*
TSC / Auto-ID	0x1103	4355
Zebra Technologies	0x0A5F	2655
Godex	0x1D90	7568
Bixolon	0x1504	5380
Epson	0x04B8	1208
Citizen	0x1D90	7568
*/
