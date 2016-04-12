04.06.2016

iOS Icons:

For App Icons to be Retina compatible, there must be a larger version with "@2x" at the end of the filename. (e.g. "myIcon@2x.png").
@2x image versions should be, literally, twice as big in either dimension. (e.g. 72x72 -> 144x144)

iOS applies a series of masks to the unaltered square icon, giving it rounded edges, gloss, shadow, etc.
In general, the radius of the rounded edge has a 1:6.4 ratio of radius to icon size.

iPhone/iPad Touch
57x57 icon
Corner Radius = 9

iPhone/iPad Touch @2x (Retina)
114x114 icon
Corner Radius = 18

iPad
72x72 icon
Corner Radius = 11

iPad @2x (Retina)
144x144 icon
Corner Radius = 23

iTunes Artwork
512x512 icon
Corner Radius = 80

iTunes Artwork @2x (Retina)
1024x1024 icon
Corner Radius = 180

------------------------------------------
Sources:

Corner radiuses: Best Answer via StackOverflow: http://stackoverflow.com/questions/2105289/iphone-app-icons-exact-radius/3813969

Icons in general: via Apple Documentation https://developer.apple.com/library/ios/navigation/
iOS Human Interface Guidelines
UI Design Basics::Designing for iOS
https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/index.html#//apple_ref/doc/uid/TP40006556-CH66-SW1
UI Design Basics::Icons and Graphics
https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/Iconography.html#//apple_ref/doc/uid/TP40006556-CH59-SW1
Icon and Image Design::Icon and Image Sizes
https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/IconMatrix.html#//apple_ref/doc/uid/TP40006556-CH27-SW1
Icon and Image Design::App Icon
https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/AppIcons.html#//apple_ref/doc/uid/TP40006556-CH19-SW1

-James
