name=$1
basex=$2
basey=$3

let hx=$basex*1
let xhx=($basex*4)/3
let xxhx=$basex*2
let hy=$basey*1
let xhy=($basey*4)/3
let xxhy=$basey*2

cv="/cygdrive/c/Program\ Files/ImageMagick-7.0.3-Q16/convert.exe"

echo $cv -size ${hx}x${hy}     xc:transparent drawable/$name        
echo $cv -size ${hx}x${hy}     xc:transparent drawable-hdpi/$name   
echo $cv -size ${xhx}x${xhy}   xc:transparent drawable-xhdpi/$name  
echo $cv -size ${xxhx}x${xxhy} xc:transparent drawable-xxhdpi/$name 
