using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
//boas
namespace SS_OpenCV
{
    class ImageClass
    {
        /// <summary>
        /// Function that solves the puzzle
        /// </summary>
        /// <param name="img">Input/Output image</param>
        /// <param name="imgCopy">Image Copy</param>
        /// <param name="Pieces_positions">List of positions (Left-x,Top-y,Right-x,Bottom-y) of all detected pieces</param>
        /// <param name="Pieces_angle">List of detected pieces' angles</param>
        /// <param name="level">Level of image</param>
        public static Image<Bgr, byte> puzzle(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, out List<int[]> Pieces_positions, out List<int> Pieces_angle, int level)
        {
            Image<Bgr, byte> dummyImg = img.Copy();
            // Pieces_positions = imageFinder(dummyImg);
            // int[] piece_vector = new int[4];
            // piece_vector[0] = 65;   // x- Top-Left 
            // piece_vector[1] = 385;  // y- Top-Left
            // piece_vector[2] = 1089; // x- Bottom-Right
            // piece_vector[3] = 1411; // y- Bottom-Right
            //Pieces_positions.Add(piece_vector);

            Pieces_angle = new List<int>(); 
            Pieces_angle.Add(0); // angle

            // MIplImage mStart = img.MIplImage;
            //Pieces_positions = imageFinder(dummyImg);

            if (level == 1)
            {                
                Pieces_positions = imageFinderLevel1(dummyImg);
                dummyImg = joinPiecesLevel1(Pieces_positions, img);
            }

            else if (level == 2)
            {
                Pieces_positions = imageFinderLevel1(dummyImg);
                dummyImg = joinPiecesLevel2(Pieces_positions, img);
            }

            else
            {

            }

            return dummyImg;
        }

        public static Image<Bgr, byte> joinPiecesLevel2(List<int[]> pieces_positions, Image<Bgr, byte> img) {
            Image<Bgr, byte> dummyImg = img.Copy();

            //Array for each piece, with the index of the pieces it is connected to,
            //The order of the sides should be [top, right, bottom, left]
            //If a side isnt connected it should have the value -1
            List<int[]> jointSides = new List<int[]>();

            //Initiate the array with -1 for each side of each piece of the pieces_positions
            //Meaning no pieces are connected together            
            joinedSides = initJoinedSides(pieces_positions);

            //Lower proximity value means higher degree of similarity             
            List<int[]> proximityValues = new List<int[]>();

            proximityValues = initProximityValues(pieces_positions);            

            for (int i = 0; i < Pieces_positions.Count - 1; i++)
            {
                for (int j = i + 1; j < Pieces_positions.Count; j++)
                    {
                        /*if (canJoinPieces(Pieces_positions[i], Pieces_positions[j])) {
                            
                            joinPieces(Pieces_positions[i], Pieces_positions[j]);
                            // if(joinsCount == Pieces_positions.Count-1) {
                            // }
                        }*/

                        //check if top is already joined
                        if (canJoinTopWithBottom(pieces_positions[i], pieces_positions[j]))
                        {

                        }

                        //top with others bottom
                        /* if(canJoinTopWithBottom(Pieces_positions[i], Pieces_positions[j])) {

                        }*/
                        //right with others left

                        //bottom with others top

                        //left with others right
                    }
                }                            

            return dummyImg;                        
        }

        public static Image<Bgr, byte> joinPiecesLevel1 (List<int[]> pieces_positions, Image<Bgr, byte> img) {
            Image<Bgr, byte> dummyImg = img.Copy();
            int widthPiece1 = pieces_positions[0][2] - pieces_positions[0][0];
            int heightPiece1 = pieces_positions[0][3] - pieces_positions[0][1];
            int widthPiece2 = pieces_positions[1][2] - pieces_positions[1][0];
            int heightPiece2 = pieces_positions[1][3] - pieces_positions[1][1];
            int[] differencesLeftRight = null;
            int[] differencesTopBottom = null;                                                

            if(widthPiece1 == widthPiece2 && heightPiece1 == heightPiece2){
                //Index0 is the difference between left of image1 and right of image2
                //Index1 is the difference between right of image1 and left of image2
                //Index2 is the difference between top of image1 and bottom of image2
                //Index3 is the difference between bottom of image1 and top of image2                
                int[] differences = new int[4];
                differencesLeftRight =  checkLeftRightSides(pieces_positions, img);
                differencesTopBottom = checkTopBottom(pieces_positions, img);
                differences[0] = differencesLeftRight[0];
                differences[1] = differencesLeftRight[1];
                differences[2] = differencesTopBottom[0];
                differences[3] = differencesTopBottom[1];
                
                int leastDifference = leastDifference(diferences);

                if(leastDifference == 0){
                    dummyImg = joinLeft1Right2();
                } else if(leastDifference == 1){
                    dummyImg = joinRight1Left2();                    
                } else if(leastDifference == 2){
                    dummyImg = joinTop1Bottom2();
                } else{
                    dummyImg = joinBottom1Top2();                    
                }
                                                                
            } else if (widthPiece1 == widthPiece2) {
                //Index0 is the difference between top of image1 and bottom of image2
                //Index1 is the difference between bottom of image1 and top of image2                            
                differencesTopBottom = checkTopBottom(pieces_positions, img);

                if(differencesTopBottom[0] < differencesTopBottom[1]){
                    dummyImg = joinTop1Bottom2();
                } else{
                    dummyImg = joinBottom1Top2();
                }
                
            } else {
                //Index0 is the difference between left of image1 and right of image2
                //Index1 is the difference between right of image1 and left of image2                
                differencesLeftRight = checkLeftRightSides(pieces_positions, img);

                if(differencesLeftRight[0] < differencesLeftRight[1]){
                    dummyImg = joinLeft1Right2();
                } else{
                    dummyImg = joinRight1Left2();
                }                                
            }
            
            return dummyImg;                
        }

        private static int leastDifference(int[] differences){
            int smallestValue = differences[0];
            int smallestIndex = 0;
            
            for(int i=1; i < differences.Length; i++){
                if(differences[i] < smallestValue){
                    smallestIndex = i;
                    smallestValue = differences[i];                    
                }
            }

            return smallestIndex;                            
        }            
            //Meaning no pieces are connected together            
            joinedSides = initJoinedSides(pieces_positions);

            //Lower proximity value means higher degree of similarity             
            List<int[]> proximityValues = new List<int[]>();

            proximityValues = initProximityValues(pieces_positions);            

            for (int i = 0; i < Pieces_positions.Count - 1; i++)
            {
                for (int j = i + 1; j < Pieces_positions.Count; j++)
                    {
                        /*if (canJoinPieces(Pieces_positions[i], Pieces_positions[j])) {
                            
                            joinPieces(Pieces_positions[i], Pieces_positions[j]);
                            // if(joinsCount == Pieces_positions.Count-1) {
                            // }
                        }*/

                        //check if top is already joined
                        if (canJoinTopWithBottom(pieces_positions[i], pieces_positions[j]))
                        {

                        }

                        //top with others bottom
                        /* if(canJoinTopWithBottom(Pieces_positions[i], Pieces_positions[j])) {

                        }*/
                        //right with others left

                        //bottom with others top

                        //left with others right
                    }
                }                            

            return dummyImg;                        

        public static int[] checkLeftRightSides(List<int[]> pieces_positions, Image<Bgr,byte> img) {
            unsafe {
                MIplImage m = img.MIplImage;
                int x1Start = pieces_positions[0][0];
                int y1Start = pieces_positions[0][1];

                int x2Start = pieces_positions[1][0];
                int y2Start = pieces_positions[1][1];

                int x1End = pieces_positions[0][2];
                int x2End = pieces_positions[1][2];

                int y1End = pieces_positions[0][3];
                //int y2End = pieces_positions[1][3];

                int[] differencesLeftRight = new int[2];
                int width = m.width;
                int height = m.height;
                int widthStep = m.widthStep;
                int nChan = m.nChannels;
                byte* dataPtr1 = (byte*)m.imageData.ToPointer();
                byte* dataPtr2 = (byte*)m.imageData.ToPointer();

                //Compare left of image1 to right of image2
                dataPtr1 += widthStep * y1Start + nChan * x1Start;
                dataPtr2 += widthStep * y2Start + nChan * x2End;
                for(int y=y1Start;y<y1End;y++) {
                    differencesLeftRight[0] += (dataPtr2[0] - dataPtr1[0]) + (dataPtr2[1] - dataPtr1[1]) + (dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += widthStep;
                    dataPtr2 += widthStep;
                }

                //Compare right of image1 to left of image2
                dataPtr1 += widthStep * y1Start + nChan * x1End;
                dataPtr2 += widthStep * y2Start + nChan * x2Start;
                for(int y=y1Start;y<y1End;y++) {
                    differencesLeftRight[1] += (dataPtr2[0] - dataPtr1[0]) + (dataPtr2[1] - dataPtr1[1]) + (dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += widthStep;
                    dataPtr2 += widthStep;
                }
                return differencesLeftRight;
            }
        }

        public static int[] checkTopBottom(List<int[]> pieces_positions, Image<Bgr,byte> img) {
            unsafe {
                MIplImage m = img.MIplImage;
                int x1Start = pieces_positions[0][0];
                int y1Start = pieces_positions[0][1];

                int x2Start = pieces_positions[1][0];
                int y2Start = pieces_positions[1][1];

                int x1End = pieces_positions[0][2];
                //int x2End = pieces_positions[1][2];

                int y1End = pieces_positions[0][3];
                int y2End = pieces_positions[1][3];

                int[] differencesTopBottom = new int[2];
                int width = m.width;
                int height = m.height;
                int widthStep = m.widthStep;
                int nChan = m.nChannels;
                byte* dataPtr1 = (byte*)m.imageData.ToPointer();
                byte* dataPtr2 = (byte*)m.imageData.ToPointer();

                //Compare top of image1 to bottom of image2
                dataPtr1 += widthStep * y1Start + nChan * x1Start;
                dataPtr2 += widthStep * y2End + nChan * x2Start;
                for(int x=x1Start;x<x1End;x++) {
                    differencesTopBottom[0] += (dataPtr2[0] - dataPtr1[0]) + (dataPtr2[1] - dataPtr1[1]) + (dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += nChan;
                    dataPtr2 += nChan;
                }

                //Compare bottom of image1 to top of image2
                dataPtr1 += widthStep * y1End + nChan * x1Start;
                dataPtr2 += widthStep * y2Start + nChan * x2Start;
                for(int x=x1Start;x<x1End;x++) {
                    differencesTopBottom[1] += (dataPtr2[0] - dataPtr1[0]) + (dataPtr2[1] - dataPtr1[1]) + (dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += nChan;
                    dataPtr2 += nChan;
                }
                return differencesTopBottom;
            }
        }

        public static List<int[]> imageFinderLevel1(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage mStart = img.MIplImage;
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                int width = img.Width;
                int height = img.Height;
                int[,] matrix = new int[width, height];
                int changes = 1;
                int bBack = dataPtr[0];
                int gBack = dataPtr[1];
                int rBack = dataPtr[2];
                int numberImages = 0;
                // int[,] imagesCoords;
                List<int[]> Pieces_positions = new List<int[]>();

                int nChan = mStart.nChannels;
                int padding = mStart.widthStep - mStart.nChannels * mStart.width;
                int widthStep = mStart.widthStep;
                int x, y;
                int left, bottom, right, top, label;
                
                while (changes == 1)
                {
                    dataPtr = (byte*)mStart.imageData.ToPointer();
                    dataPtr += nChan + widthStep;
                    changes = 0;

                    for (y = 1; y < height-1; y++)
                    {
                        for (x = 1; x < width-1; x++)
                        {
                            
                            if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack)
                            {
                                left = matrix[x - 1, y];
                                top = matrix[x, y - 1];
                                right = matrix[x + 1, y];
                                bottom = matrix[x, y + 1];
                                label = width * height;

                                if (top != 0 && top < label)
                                {
                                    label = top;
                                }
                                if (left != 0 && left < label)
                                {
                                    label = left;
                                }
                                if (right != 0 && right < label)
                                {
                                    label = right;
                                }
                                if (bottom != 0 && bottom < label)
                                {
                                    label = bottom;
                                }
                                if (label == width * height) //New image found
                                {
                                    Pieces_positions.Add(new int[4]);
                                    label = ++numberImages;
                                }
                                if (matrix[x, y] != label) //If matrix was changed
                                {
                                    changes = 1;
                                    matrix[x, y] = label;
                                }
                            }

                            dataPtr += nChan;
                        }
                    
                        dataPtr += padding + nChan * 2;
                    }
                }
                
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan;

                //Top Margin
                for(x = 1; x < width - 1; x++){

                    if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
                        left = matrix[x - 1, 0];
                        bottom = matrix[x, 1];
                        label = width * height;

                        if (left != 0 && left < label)
                        {
                            label = left;
                        }

                        if (bottom != 0 && bottom < label)
                        {
                            label = bottom;
                        }        

                        matrix[x, 0] = label; 
                    }
                    dataPtr += nChan;                                                                      
                }          


                //Bottom Margin
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height-1) + nChan;

                for(x = 1; x < width - 1; x++) {

                    if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
                        left = matrix[x - 1,height-1];
                        top = matrix[x, height - 2];
                        label = width * height;                    

                        if (left != 0 && left < label)
                        {
                            label = left;
                        }

                        if (top != 0 && top < label)
                        {
                            label = top;
                        }

                        matrix[x, height - 1] = label;  
                    }                                                                        
                }            
  
                //Left Margin  
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep;   
                x = width-1;                                          
                for(y = 1; y < height-1; y++) {

                    if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
                        top = matrix[x, y];
                        right = matrix[x + 1, y];
                        label = width * height;                    

                        if (top != 0 && top < label)
                        {
                            label = top;
                        }

                        if (right != 0 && right < label)
                        {
                            label = right;
                        }   

                        matrix[0, y] = label;  
                    }                                                                                                                                         
                }     

                //Right Margin
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep + nChan * (width - 1);
                x = width-1;                                        
                for(y = 1; y < height-1; y++) {    

                    if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
                        left = matrix[x - 1,y];
                        top = matrix[x, y];
                        label = width * height;                    

                        if (left != 0 && left < label)
                        {
                            label = left;
                        }

                        if (top != 0 && top < label)
                        {
                            label = top;
                        }

                        matrix[width - 1, y] = label;  
                    }
                }

                //Superior Right Corner
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan * (width-1);
                left = matrix[x-1, 0];
                bottom = matrix[x, 1];
                label = width * height;
                
                if (left != 0 && left < label)
                {
                    label = left;
                }

                if (bottom != 0 && bottom < label)
                {
                    label = bottom;
                }

                //Inferior Left Corner
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height-1);
                top = matrix[0, y-1];
                right = matrix[1, y];
                label = width * height;
                if (top != 0 && top < label)
                {
                    label = top;
                }
                if (right != 0 && right < label)
                {
                    label = right;
                }

                //Inferior Right Corner 
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height-1) + nChan * (height-1);                             
                top = matrix[x, y-1];
                left = matrix[x - 1, y];
                label = width * height;
                if (top != 0 && top < label)
                {
                    label = top;
                }
                if (left != 0 && left < label)
                {
                    label = left;
                }


                //Construir inicio e fim de cada imagem
                int currentLabel = 0;

                for (y = 0; y < height && numberImages != currentLabel; y++)
                {
                    for (x = 0; x < width && numberImages != currentLabel; x++)
                    {
                        if (matrix[x, y] != currentLabel)
                        {
                            Pieces_positions[currentLabel][0] = x; //xStart
                            Pieces_positions[currentLabel][1] = y; //yStart
                            currentLabel++;
                        }
                    }
                }

                currentLabel = 0;
                for (y = height - 1; y >= 0 && numberImages != currentLabel ; y--)
                {
                    for (x = width - 1; x >= 0 && numberImages != currentLabel; x--)
                    {
                        if (matrix[x, y] != currentLabel)
                        {
                            Pieces_positions[currentLabel][2] = x; //xEnd
                            Pieces_positions[currentLabel][3] = y; //yEnd
                            currentLabel++;
                        }
                    }
                }
                return Pieces_positions;
            }
        }

        public static Image<Bgr, byte>[] createImages(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage mStart = img.MIplImage;
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();

                int nChan = mStart.nChannels;
                int padding = mStart.widthStep - mStart.nChannels * mStart.width;
                int widthStep = mStart.widthStep;

                //Alterar esta parte
                int[] piece_vector = new int[8];
                piece_vector[0] = 65;   // x- Top-Left1
                piece_vector[1] = 385;  // y- Top-Left1
                piece_vector[2] = 1089; // x- Bottom-Right1
                piece_vector[3] = 1411; // y- Bottom-Right1

                piece_vector[4] = 65;   // x- Top-Left2 
                piece_vector[5] = 385;  // y- Top-Left2
                piece_vector[6] = 1089; // x- Bottom-Right2
                piece_vector[7] = 1411; // y- Bottom-Right2

                Image<Bgr, byte> piece1 = new Image<Bgr, byte>(piece_vector[2] - piece_vector[0], piece_vector[3] - piece_vector[1]);
                Image<Bgr, byte> piece2 = new Image<Bgr, byte>(piece_vector[6] - piece_vector[4], piece_vector[7] - piece_vector[5]);

                MIplImage piece = piece1.MIplImage;
                byte* pieceData = (byte*)piece.imageData.ToPointer();

                dataPtr += nChan * piece_vector[0] + widthStep * piece_vector[1];

                for (int y = piece_vector[1]; y < piece_vector[3]; y++)
                {
                    for (int x = piece_vector[0]; x < piece_vector[2]; x++)
                    {
                        pieceData = dataPtr;
                        pieceData += nChan;
                        dataPtr += nChan;
                    }

                    pieceData += padding;
                    dataPtr += padding;
                }

                piece = piece2.MIplImage;
                pieceData = (byte*)piece.imageData.ToPointer();
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan * piece_vector[4] + widthStep * piece_vector[5];

                for (int y = piece_vector[5]; y < piece_vector[7]; y++)
                {
                    for (int x = piece_vector[4]; x < piece_vector[6]; x++)
                    {
                        pieceData = dataPtr;
                        pieceData += nChan;
                        dataPtr += nChan;
                    }

                    pieceData += padding;
                    dataPtr += padding;
                }

                Image<Bgr, byte>[] ret = new Image<Bgr, byte>[2];
                ret[0] = piece1;
                ret[1] = piece2;
                return ret;
            }
        }

        public static int compareSides(Image<Bgr, byte> peace1, Image<Bgr, byte> peace2)
        {
            return -1;
        }

        //pre widths iguais etc e height
        public static int compareTopAndBottom(Image<Bgr, byte> img, int[] piece_vector, int[] piece_vector2)
        {
            unsafe
            {
                int difference = 0;
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                byte* dataPtr2 = (byte*)m.imageData.ToPointer();

                int xTopLeft = piece_vector[0];
                int width = piece_vector[2] - xTopLeft + 1;

                int x2BottomLeft = piece_vector2[0];

                for (int i = xTopLeft; i < width; i++)
                {
                    difference = dataPtr[0] - dataPtr2[0] + dataPtr[1] - dataPtr2[1] + dataPtr[2] - dataPtr2[2];
                    dataPtr += 3;
                    dataPtr2 += 3;
                }
                return (int)Math.Abs(difference);
            }
        }

        public static void Scale(Image<Bgr, byte> transformedImg, Image<Bgr, byte> startingImg, float scaleFactor)
        {
            unsafe
            {
                MIplImage mStart = startingImg.MIplImage;
                MIplImage mTransformed = transformedImg.MIplImage;

                byte* dataPtr = (byte*)mStart.imageData.ToPointer(); // Pointer to the image
                byte* transformedPtr = (byte*)mTransformed.imageData.ToPointer();

                int width = startingImg.Width;
                int height = startingImg.Height;
                int nChan = mStart.nChannels; // number of channels - 3
                int padding = mStart.widthStep - mStart.nChannels * mStart.width; // alinhament bytes (padding)
                int x, y;
                byte blue, green, red;
                int origX, origY;

                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        origX = (int)Math.Round(x / scaleFactor);
                        origY = (int)Math.Round(y / scaleFactor);

                        if (origX > mStart.width || origX < 0 || origY >= height || origY < 0)
                        {
                            blue = 0;
                            red = 0;
                            green = 0;
                        }

                        else
                        {
                            blue = (byte)(dataPtr + origY * mStart.widthStep + origX * nChan)[0];
                            green = (byte)(dataPtr + origY * mStart.widthStep + origX * nChan)[1];
                            red = (byte)(dataPtr + origY * mStart.widthStep + origX * nChan)[2];
                        }

                        (transformedPtr + (y) * mStart.widthStep + (x) * nChan)[0] = blue;
                        (transformedPtr + (y) * mStart.widthStep + (x) * nChan)[1] = green;
                        (transformedPtr + (y) * mStart.widthStep + (x) * nChan)[2] = red;
                    }
                }
            }
        }

        public static void Scale_point_xy(Image<Bgr, byte> transformedImg, Image<Bgr, byte> startingImg, float scaleFactor, int centerX, int centerY)
        {
            unsafe
            {
                MIplImage mStart = startingImg.MIplImage;
                MIplImage mTransformed = transformedImg.MIplImage;

                byte* dataPtr = (byte*)mStart.imageData.ToPointer(); // Pointer to the image
                byte* transformedPtr = (byte*)mTransformed.imageData.ToPointer();

                int width = startingImg.Width;
                int height = startingImg.Height;
                int nChan = mStart.nChannels; // number of channels - 3
                int padding = mStart.widthStep - mStart.nChannels * mStart.width; // alinhament bytes (padding)
                double newX, newY;
                byte blue, green, red;
                int x, y;
                int origX, origY;

                newX = (centerX - (width / 2) / scaleFactor);
                newY = (centerY - (height / 2) / scaleFactor);

                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        origX = (int)Math.Round(x / scaleFactor + newX);
                        origY = (int)Math.Round(y / scaleFactor + newY);

                        if (origX > mStart.width || origX < 0 || origY >= height || origY < 0)
                        {
                            blue = 0;
                            red = 0;
                            green = 0;
                        }

                        else
                        {
                            blue = (byte)(dataPtr + origY * mStart.widthStep + origX * nChan)[0];
                            green = (byte)(dataPtr + origY * mStart.widthStep + origX * nChan)[1];
                            red = (byte)(dataPtr + origY * mStart.widthStep + origX * nChan)[2];
                        }

                        (transformedPtr + (y) * mStart.widthStep + (x) * nChan)[0] = blue;
                        (transformedPtr + (y) * mStart.widthStep + (x) * nChan)[1] = green;
                        (transformedPtr + (y) * mStart.widthStep + (x) * nChan)[2] = red;
                    }
                }
            }
        }


        /// <summary>
        /// Image Negative using EmguCV library
        /// Slower method
        /// </summary>
        /// <param name="img">Image</param>
        public static void Negative(Image<Bgr, byte> img)
        {
            int x, y;

            Bgr aux;
            for (y = 0; y < img.Height; y++)
            {
                for (x = 0; x < img.Width; x++)
                {
                    // acesso indirecto : mais lento
                    aux = img[y, x];
                    img[y, x] = new Bgr(255 - aux.Blue, 255 - aux.Green, 255 - aux.Red);
                }
            }
        }

        public static void RedChannel(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)

                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        byte* red = dataPtr + 2;
                        dataPtr[0] = *red;
                        dataPtr[1] = *red;

                        dataPtr += nChan;
                    }
                    dataPtr += padding;
                }
            }
        }

        public static void NegativeDirect(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)

                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        dataPtr[0] = (byte)(255 - dataPtr[0]);
                        dataPtr[1] = (byte)(255 - dataPtr[1]);
                        dataPtr[2] = (byte)(255 - dataPtr[2]);

                        dataPtr += nChan;
                    }
                    dataPtr += padding;
                }
            }
        }

        public static void BrightContrast(Image<Bgr, byte> img, int brightness, double contrast)
        { //pix_mod(x,y) = contraste * pixel(x,y) + brilho
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                //byte blue, green, red;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y;
                double modifiedPixel;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            // store in the image
                            modifiedPixel = (contrast * dataPtr[0] + brightness);
                            if (modifiedPixel > 255)
                            {
                                dataPtr[0] = 255;
                            }
                            else if (modifiedPixel < 0)
                            {
                                dataPtr[0] = 0;
                            }
                            else
                            {
                                dataPtr[0] = (byte)Math.Round(modifiedPixel);
                            }

                            modifiedPixel = (contrast * dataPtr[1] + brightness);
                            if (modifiedPixel > 255)
                            {
                                dataPtr[1] = 255;
                            }
                            else if (modifiedPixel < 0)
                            {
                                dataPtr[1] = 0;
                            }
                            else
                            {
                                dataPtr[1] = (byte)Math.Round(modifiedPixel);
                            }

                            modifiedPixel = (contrast * dataPtr[2] + brightness);
                            if (modifiedPixel > 255)
                            {
                                dataPtr[2] = 255;
                            }
                            else if (modifiedPixel < 0)
                            {
                                dataPtr[2] = 0;
                            }
                            else
                            {
                                dataPtr[2] = (byte)Math.Round(modifiedPixel);
                            }

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Convert to gray
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void ConvertToGray(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte blue, green, red, gray;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            // convert to gray
                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);

                            // store in the image
                            dataPtr[0] = gray;
                            dataPtr[1] = gray;
                            dataPtr[2] = gray;

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        public static void Translation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int dx, int dy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = mCopy.widthStep - mCopy.nChannels * mCopy.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer();
                int nChannels = mCopy.nChannels;

                if (nChannels == 3)
                {

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (x - dx < 0 || y - dy < 0 || x - dx >= width || y - dy >= height) //Porque >= qu??
                            {
                                dataPtr[0] = 0;
                                dataPtr[1] = 0;
                                dataPtr[2] = 0;
                            }
                            else
                            {
                                dataPtr[0] = (byte)(dataPtrCopy + (y - dy) * m.widthStep + (x - dx) * nChannels)[0];
                                dataPtr[1] = (byte)(dataPtrCopy + (y - dy) * m.widthStep + (x - dx) * nChannels)[1];
                                dataPtr[2] = (byte)(dataPtrCopy + (y - dy) * m.widthStep + (x - dx) * nChannels)[2];
                            }

                            dataPtr += nChannels;
                        }
                        dataPtr += padding;
                    }
                }
            }
        }

        public static void Rotation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float angle)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = mCopy.widthStep - mCopy.nChannels * mCopy.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer();
                int nChannels = mCopy.nChannels;

                double valueOfCos = Math.Cos(angle);
                double valueOfSin = Math.Sin(angle);
                double halfWidth = width / 2.0;
                double halfHeight = height / 2.0;
                int newX, newY;

                if (nChannels == 3)
                {

                    for (int y = 0; y < height; y++)
                    {

                        for (int x = 0; x < width; x++)
                        {

                            newX = (int)Math.Round(((x - halfWidth) * valueOfCos - (halfHeight - y) * valueOfSin + halfWidth));
                            newY = (int)Math.Round((halfHeight - (x - halfWidth) * valueOfSin - (halfHeight - y) * valueOfCos));

                            if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                            {
                                dataPtr[0] = (byte)(dataPtrCopy + newY * m.widthStep + newX * nChannels)[0];
                                dataPtr[1] = (byte)(dataPtrCopy + newY * m.widthStep + newX * nChannels)[1];
                                dataPtr[2] = (byte)(dataPtrCopy + newY * m.widthStep + newX * nChannels)[2];
                            }
                            else
                            {
                                dataPtr[0] = 0;
                                dataPtr[1] = 0;
                                dataPtr[2] = 0;
                            }

                            dataPtr += nChannels;
                        }
                        dataPtr += padding;
                    }
                }
            }
        }

        public static void Mean(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        { //3x3
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = mCopy.widthStep - mCopy.nChannels * mCopy.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer(), dataPtr2 = dataPtr;
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer(), dataPtrCopy2 = dataPtrCopy;
                int nChannels = mCopy.nChannels;
                double numberOfPixelsToSum = 9.0;

                if (nChannels == 3)
                {
                    int[] tempArray = new int[3];
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            dataPtr2 = (dataPtr + y * m.widthStep + x * nChannels);
                            dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + x * nChannels);

                            tempArray[0] = (int)(Math.Round((
                                (dataPtrCopy2 - m.widthStep - nChannels)[0] +
                                (dataPtrCopy2 - m.widthStep)[0] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[0] +
                                (dataPtrCopy2 - nChannels)[0] +
                                (dataPtrCopy2)[0] +
                                (dataPtrCopy2 + nChannels)[0] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[0] +
                                (dataPtrCopy2 + m.widthStep)[0] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[0]) / numberOfPixelsToSum));

                            tempArray[1] = (int)(Math.Round((
                                (dataPtrCopy2 - m.widthStep - nChannels)[1] +
                                (dataPtrCopy2 - m.widthStep)[1] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[1] +
                                (dataPtrCopy2 - nChannels)[1] +
                                (dataPtrCopy2)[1] +
                                (dataPtrCopy2 + nChannels)[1] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[1] +
                                (dataPtrCopy2 + m.widthStep)[1] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[1]) / numberOfPixelsToSum));

                            tempArray[2] = (int)(Math.Round((
                                (dataPtrCopy2 - m.widthStep - nChannels)[2] +
                                (dataPtrCopy2 - m.widthStep)[2] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[2] +
                                (dataPtrCopy2 - nChannels)[2] +
                                (dataPtrCopy2)[2] +
                                (dataPtrCopy2 + nChannels)[2] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[2] +
                                (dataPtrCopy2 + m.widthStep)[2] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[2]) / numberOfPixelsToSum));

                            if (tempArray[0] > 255)
                            {
                                dataPtr2[0] = 255;
                            }
                            else if (tempArray[0] < 0)
                            {
                                dataPtr2[0] = 0;
                            }
                            else
                            {
                                dataPtr2[0] = (byte)tempArray[0];
                            }

                            if (tempArray[1] > 255)
                            {
                                dataPtr2[1] = 255;
                            }
                            else if (tempArray[1] < 0)
                            {
                                dataPtr2[1] = 0;
                            }
                            else
                            {
                                dataPtr2[1] = (byte)tempArray[1];
                            }

                            if (tempArray[2] > 255)
                            {
                                dataPtr2[2] = 255;
                            }
                            else if (tempArray[2] < 0)
                            {
                                dataPtr2[2] = 0;
                            }
                            else
                            {
                                dataPtr2[2] = (byte)tempArray[2];
                            }

                        }
                    }
                }
            }
        }

        public static void NonUniform(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float[,] matrix, float matrixWeight)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer(), dataPtr2 = dataPtr;
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer(), dataPtrCopy2 = dataPtrCopy;
                int nChannels = m.nChannels;

                if (nChannels == 3)
                {
                    int[] tempArray = new int[3];
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            dataPtr2 = (dataPtr + y * m.widthStep + x * nChannels);
                            dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + x * nChannels);

                            tempArray[0] = (int)Math.Round(
                                ((dataPtrCopy2 - m.widthStep - nChannels)[0] * matrix[0, 0] +
                                (dataPtrCopy2 - m.widthStep)[0] * matrix[0, 1] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[0] * matrix[0, 2] +
                                (dataPtrCopy2 - nChannels)[0] * matrix[1, 0] +
                                (dataPtrCopy2)[0] * matrix[1, 1] +
                                (dataPtrCopy2 + nChannels)[0] * matrix[1, 2] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[0] * matrix[2, 0] +
                                (dataPtrCopy2 + m.widthStep)[0] * matrix[2, 1] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[0] * matrix[2, 2])
                                / matrixWeight);

                            tempArray[1] = (int)Math.Round(
                                ((dataPtrCopy2 - m.widthStep - nChannels)[1] * matrix[0, 0] +
                                (dataPtrCopy2 - m.widthStep)[1] * matrix[0, 1] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[1] * matrix[0, 2] +
                                (dataPtrCopy2 - nChannels)[1] * matrix[1, 0] +
                                (dataPtrCopy2)[1] * matrix[1, 1] +
                                (dataPtrCopy2 + nChannels)[1] * matrix[1, 2] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[1] * matrix[2, 0] +
                                (dataPtrCopy2 + m.widthStep)[1] * matrix[2, 1] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[1] * matrix[2, 2])
                                / matrixWeight);

                            tempArray[2] = (int)Math.Round(
                               ((dataPtrCopy2 - m.widthStep - nChannels)[2] * matrix[0, 0] +
                                (dataPtrCopy2 - m.widthStep)[2] * matrix[0, 1] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[2] * matrix[0, 2] +
                                (dataPtrCopy2 - nChannels)[2] * matrix[1, 0] +
                                (dataPtrCopy2)[2] * matrix[1, 1] +
                                (dataPtrCopy2 + nChannels)[2] * matrix[1, 2] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[2] * matrix[2, 0] +
                                (dataPtrCopy2 + m.widthStep)[2] * matrix[2, 1] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[2] * matrix[2, 2])
                                / matrixWeight);


                            if (tempArray[0] > 255)
                            {
                                dataPtr2[0] = 255;
                            }
                            else if (tempArray[0] < 0)
                            {
                                dataPtr2[0] = 0;
                            }
                            else
                            {
                                dataPtr2[0] = (byte)tempArray[0];
                            }

                            if (tempArray[1] > 255)
                            {
                                dataPtr2[1] = 255;
                            }
                            else if (tempArray[1] < 0)
                            {
                                dataPtr2[1] = 0;
                            }
                            else
                            {
                                dataPtr2[1] = (byte)tempArray[1];
                            }

                            if (tempArray[2] > 255)
                            {
                                dataPtr2[2] = 255;
                            }
                            else if (tempArray[2] < 0)
                            {
                                dataPtr2[2] = 0;
                            }
                            else
                            {
                                dataPtr2[2] = (byte)tempArray[2];
                            }

                        }
                    }
                }
            }
        }

        public static void Sobel(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {//USAR RELATIVO
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer(), dataPtr2 = dataPtr;
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer(), dataPtrCopy2 = dataPtrCopy;
                int nChannels = m.nChannels;

                if (nChannels == 3)
                {
                    int[] tempArray = new int[3];
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            dataPtr2 = (dataPtr + y * m.widthStep + x * nChannels);
                            dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + x * nChannels);

                            tempArray[0] = Math.Abs(
                                ((dataPtrCopy2 - m.widthStep - nChannels)[0] +
                                2 * (dataPtrCopy2 - nChannels)[0] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[0]) -

                                ((dataPtrCopy2 - m.widthStep + nChannels)[0] +
                                2 * (dataPtrCopy2 + nChannels)[0] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[0]) +

                               Math.Abs(
                                ((dataPtrCopy2 + m.widthStep - nChannels)[0] +
                                2 * (dataPtrCopy2 + m.widthStep)[0] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[0]) -

                                ((dataPtrCopy2 - m.widthStep - nChannels)[0] +
                                2 * (dataPtrCopy2 - m.widthStep)[0] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[0])));

                            tempArray[1] = Math.Abs(
                                ((dataPtrCopy2 - m.widthStep - nChannels)[1] +
                                2 * (dataPtrCopy2 - nChannels)[1] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[1]) -

                                ((dataPtrCopy2 - m.widthStep + nChannels)[1] +
                                2 * (dataPtrCopy2 + nChannels)[1] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[1]) +

                               Math.Abs(
                                ((dataPtrCopy2 + m.widthStep - nChannels)[1] +
                                2 * (dataPtrCopy2 + m.widthStep)[1] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[1]) -

                                ((dataPtrCopy2 - m.widthStep - nChannels)[1] +
                                2 * (dataPtrCopy2 - m.widthStep)[1] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[1])));

                            tempArray[2] = Math.Abs(
                                ((dataPtrCopy2 - m.widthStep - nChannels)[2] +
                                2 * (dataPtrCopy2 - nChannels)[2] +
                                (dataPtrCopy2 + m.widthStep - nChannels)[2]) -

                                ((dataPtrCopy2 - m.widthStep + nChannels)[2] +
                                2 * (dataPtrCopy2 + nChannels)[2] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[2]) +

                               Math.Abs(
                                ((dataPtrCopy2 + m.widthStep - nChannels)[2] +
                                2 * (dataPtrCopy2 + m.widthStep)[2] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[2]) -

                                ((dataPtrCopy2 - m.widthStep - nChannels)[2] +
                                2 * (dataPtrCopy2 - m.widthStep)[2] +
                                (dataPtrCopy2 - m.widthStep + nChannels)[2])));


                            if (tempArray[0] > 255)
                            {
                                dataPtr2[0] = 255;
                            }
                            else if (tempArray[0] < 0)
                            {
                                dataPtr2[0] = 0;
                            }
                            else
                            {
                                dataPtr2[0] = (byte)tempArray[0];
                            }

                            if (tempArray[1] > 255)
                            {
                                dataPtr2[1] = 255;
                            }
                            else if (tempArray[1] < 0)
                            {
                                dataPtr2[1] = 0;
                            }
                            else
                            {
                                dataPtr2[1] = (byte)tempArray[1];
                            }

                            if (tempArray[2] > 255)
                            {
                                dataPtr2[2] = 255;
                            }
                            else if (tempArray[2] < 0)
                            {
                                dataPtr2[2] = 0;
                            }
                            else
                            {
                                dataPtr2[2] = (byte)tempArray[2];
                            }

                        }
                    }

                    //Left-top pixel
                    dataPtr2 = (dataPtr + height * m.widthStep + 0 * nChannels);
                    dataPtrCopy2 = (dataPtrCopy + height * m.widthStep + 0 * nChannels);

                    tempArray[0] = Math.Abs(
                            ((dataPtrCopy2)[0] +
                            2 * (dataPtrCopy2)[0] +
                            (dataPtrCopy2 + m.widthStep)[0]) -

                            ((dataPtrCopy2 + nChannels)[0] +
                            2 * (dataPtrCopy2 + nChannels)[0] +
                            (dataPtrCopy2 + m.widthStep + nChannels)[0]) +

                           Math.Abs(
                            ((dataPtrCopy2 + m.widthStep)[0] +
                            2 * (dataPtrCopy2 + m.widthStep)[0] +
                            (dataPtrCopy2 + m.widthStep + nChannels)[0]) -

                            ((dataPtrCopy2)[0] +
                            2 * (dataPtrCopy2)[0] +
                            (dataPtrCopy2 + nChannels)[0])));

                    tempArray[1] = Math.Abs(
                            ((dataPtrCopy2)[1] +
                            2 * (dataPtrCopy2)[1] +
                            (dataPtrCopy2 + m.widthStep)[1]) -

                            ((dataPtrCopy2 + nChannels)[1] +
                            2 * (dataPtrCopy2 + nChannels)[1] +
                            (dataPtrCopy2 + m.widthStep + nChannels)[1]) +

                           Math.Abs(
                            ((dataPtrCopy2 + m.widthStep)[1] +
                            2 * (dataPtrCopy2 + m.widthStep)[1] +
                            (dataPtrCopy2 + m.widthStep + nChannels)[1]) -

                            ((dataPtrCopy2)[1] +
                            2 * (dataPtrCopy2)[1] +
                            (dataPtrCopy2 + nChannels)[1])));

                    tempArray[2] = Math.Abs(
                                ((dataPtrCopy2)[2] +
                                2 * (dataPtrCopy2)[2] +
                                (dataPtrCopy2 + m.widthStep)[2]) -

                                ((dataPtrCopy2 + nChannels)[2] +
                                2 * (dataPtrCopy2 + nChannels)[2] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[2]) +

                               Math.Abs(
                                ((dataPtrCopy2 + m.widthStep)[2] +
                                2 * (dataPtrCopy2 + m.widthStep)[2] +
                                (dataPtrCopy2 + m.widthStep + nChannels)[2]) -

                                ((dataPtrCopy2)[2] +
                                2 * (dataPtrCopy2)[2] +
                                (dataPtrCopy2 + nChannels)[2])));


                    if (tempArray[0] > 255)
                    {
                        dataPtr2[0] = 255;
                    }
                    else if (tempArray[0] < 0)
                    {
                        dataPtr2[0] = 0;
                    }
                    else
                    {
                        dataPtr2[0] = (byte)tempArray[0];
                    }

                    if (tempArray[1] > 255)
                    {
                        dataPtr2[1] = 255;
                    }
                    else if (tempArray[1] < 0)
                    {
                        dataPtr2[1] = 0;
                    }
                    else
                    {
                        dataPtr2[1] = (byte)tempArray[1];
                    }

                    if (tempArray[2] > 255)
                    {
                        dataPtr2[2] = 255;
                    }
                    else if (tempArray[2] < 0)
                    {
                        dataPtr2[2] = 0;
                    }
                    else
                    {
                        dataPtr2[2] = (byte)tempArray[2];
                    }

                    //Right-top pixel
                    dataPtr2 = (dataPtr + height * m.widthStep + 0 * nChannels);
                    dataPtrCopy2 = (dataPtrCopy + height * m.widthStep + 0 * nChannels);

                    tempArray[0] = Math.Abs(
                            ((dataPtrCopy2 - m.widthStep - nChannels)[0] +
                            2 * (dataPtrCopy2 - nChannels)[0] +
                            (dataPtrCopy2 + m.widthStep - nChannels)[0]) -

                            ((dataPtrCopy2)[0] +
                            2 * (dataPtrCopy2)[0] +
                            (dataPtrCopy2 + m.widthStep)[0]) +

                           Math.Abs(
                            ((dataPtrCopy2 + m.widthStep - nChannels)[0] +
                            2 * (dataPtrCopy2 + m.widthStep)[0] +
                            (dataPtrCopy2 + m.widthStep)[0]) -

                            ((dataPtrCopy2 - nChannels)[0] +
                            2 * (dataPtrCopy2)[0] +
                            (dataPtrCopy2)[0])));

                    tempArray[1] = Math.Abs(
                           ((dataPtrCopy2 - m.widthStep - nChannels)[1] +
                           2 * (dataPtrCopy2 - nChannels)[1] +
                           (dataPtrCopy2 + m.widthStep - nChannels)[1]) -

                           ((dataPtrCopy2)[1] +
                           2 * (dataPtrCopy2)[1] +
                           (dataPtrCopy2 + m.widthStep)[1]) +

                          Math.Abs(
                           ((dataPtrCopy2 + m.widthStep - nChannels)[1] +
                           2 * (dataPtrCopy2 + m.widthStep)[1] +
                           (dataPtrCopy2 + m.widthStep)[1]) -

                           ((dataPtrCopy2 - nChannels)[1] +
                           2 * (dataPtrCopy2)[1] +
                           (dataPtrCopy2)[1])));

                    tempArray[2] = Math.Abs(
                           ((dataPtrCopy2 - m.widthStep - nChannels)[2] +
                           2 * (dataPtrCopy2 - nChannels)[2] +
                           (dataPtrCopy2 + m.widthStep - nChannels)[2]) -

                           ((dataPtrCopy2)[2] +
                           2 * (dataPtrCopy2)[2] +
                           (dataPtrCopy2 + m.widthStep)[2]) +

                          Math.Abs(
                           ((dataPtrCopy2 + m.widthStep - nChannels)[2] +
                           2 * (dataPtrCopy2 + m.widthStep)[2] +
                           (dataPtrCopy2 + m.widthStep)[2]) -

                           ((dataPtrCopy2 - nChannels)[2] +
                           2 * (dataPtrCopy2)[2] +
                           (dataPtrCopy2)[2])));


                    if (tempArray[0] > 255)
                    {
                        dataPtr2[0] = 255;
                    }
                    else if (tempArray[0] < 0)
                    {
                        dataPtr2[0] = 0;
                    }
                    else
                    {
                        dataPtr2[0] = (byte)tempArray[0];
                    }

                    if (tempArray[1] > 255)
                    {
                        dataPtr2[1] = 255;
                    }
                    else if (tempArray[1] < 0)
                    {
                        dataPtr2[1] = 0;
                    }
                    else
                    {
                        dataPtr2[1] = (byte)tempArray[1];
                    }

                    if (tempArray[2] > 255)
                    {
                        dataPtr2[2] = 255;
                    }
                    else if (tempArray[2] < 0)
                    {
                        dataPtr2[2] = 0;
                    }
                    else
                    {
                        dataPtr2[2] = (byte)tempArray[2];
                    }

                    //Right-down pixel
                    dataPtr2 = (dataPtr + height * m.widthStep + 0 * nChannels);
                    dataPtrCopy2 = (dataPtrCopy + height * m.widthStep + 0 * nChannels);

                    tempArray[0] = Math.Abs(
                            ((dataPtrCopy2 - m.widthStep)[0] +
                            2 * (dataPtrCopy2)[0] +
                            (dataPtrCopy2)[0]) -

                            ((dataPtrCopy2 - m.widthStep + nChannels)[0] +
                            2 * (dataPtrCopy2 + nChannels)[0] +
                            (dataPtrCopy2 + m.widthStep + nChannels)[0]) +

                           Math.Abs(
                            ((dataPtrCopy2 + m.widthStep - nChannels)[0] +
                            2 * (dataPtrCopy2 + m.widthStep)[0] +
                            (dataPtrCopy2 + m.widthStep)[0]) -

                            ((dataPtrCopy2 - nChannels)[0] +
                            2 * (dataPtrCopy2)[0] +
                            (dataPtrCopy2)[0])));

                    tempArray[1] = Math.Abs(
                           ((dataPtrCopy2 - m.widthStep - nChannels)[1] +
                           2 * (dataPtrCopy2 - nChannels)[1] +
                           (dataPtrCopy2 + m.widthStep - nChannels)[1]) -

                           ((dataPtrCopy2)[1] +
                           2 * (dataPtrCopy2)[1] +
                           (dataPtrCopy2 + m.widthStep)[1]) +

                          Math.Abs(
                           ((dataPtrCopy2 + m.widthStep - nChannels)[1] +
                           2 * (dataPtrCopy2 + m.widthStep)[1] +
                           (dataPtrCopy2 + m.widthStep)[1]) -

                           ((dataPtrCopy2 - nChannels)[1] +
                           2 * (dataPtrCopy2)[1] +
                           (dataPtrCopy2)[1])));

                    tempArray[2] = Math.Abs(
                           ((dataPtrCopy2 - m.widthStep - nChannels)[2] +
                           2 * (dataPtrCopy2 - nChannels)[2] +
                           (dataPtrCopy2 + m.widthStep - nChannels)[2]) -

                           ((dataPtrCopy2)[2] +
                           2 * (dataPtrCopy2)[2] +
                           (dataPtrCopy2 + m.widthStep)[2]) +

                          Math.Abs(
                           ((dataPtrCopy2 + m.widthStep - nChannels)[2] +
                           2 * (dataPtrCopy2 + m.widthStep)[2] +
                           (dataPtrCopy2 + m.widthStep)[2]) -

                           ((dataPtrCopy2 - nChannels)[2] +
                           2 * (dataPtrCopy2)[2] +
                           (dataPtrCopy2)[2])));


                    if (tempArray[0] > 255)
                    {
                        dataPtr2[0] = 255;
                    }
                    else if (tempArray[0] < 0)
                    {
                        dataPtr2[0] = 0;
                    }
                    else
                    {
                        dataPtr2[0] = (byte)tempArray[0];
                    }

                    if (tempArray[1] > 255)
                    {
                        dataPtr2[1] = 255;
                    }
                    else if (tempArray[1] < 0)
                    {
                        dataPtr2[1] = 0;
                    }
                    else
                    {
                        dataPtr2[1] = (byte)tempArray[1];
                    }

                    if (tempArray[2] > 255)
                    {
                        dataPtr2[2] = 255;
                    }
                    else if (tempArray[2] < 0)
                    {
                        dataPtr2[2] = 0;
                    }
                    else
                    {
                        dataPtr2[2] = (byte)tempArray[2];
                    }



                    //Last Line
                    for (int x = 0; x < width; x++)
                    {
                        dataPtr2 = (dataPtr + (height - 1) * m.widthStep + x * nChannels);
                        dataPtrCopy2 = (dataPtrCopy + (height - 1) * m.widthStep + x * nChannels);

                        tempArray[0] = Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + nChannels)[0]);
                        tempArray[1] = Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + nChannels)[1]);
                        tempArray[2] = Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + nChannels)[2]);


                        if (tempArray[0] > 255)
                        {
                            dataPtr2[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr2[0] = 0;
                        }
                        else
                        {
                            dataPtr2[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr2[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr2[1] = 0;
                        }
                        else
                        {
                            dataPtr2[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr2[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr2[2] = 0;
                        }
                        else
                        {
                            dataPtr2[2] = (byte)tempArray[2];
                        }
                    }

                    //Left Column
                    for (int y = 0; y < height; y++)
                    {
                        dataPtr2 = (dataPtr + y * m.widthStep + (width - 1) * nChannels);
                        dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + (width - 1) * nChannels);

                        tempArray[0] = Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + m.widthStep)[0]);
                        tempArray[1] = Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + m.widthStep)[1]);
                        tempArray[2] = Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + m.widthStep)[2]);


                        if (tempArray[0] > 255)
                        {
                            dataPtr2[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr2[0] = 0;
                        }
                        else
                        {
                            dataPtr2[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr2[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr2[1] = 0;
                        }
                        else
                        {
                            dataPtr2[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr2[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr2[2] = 0;
                        }
                        else
                        {
                            dataPtr2[2] = (byte)tempArray[2];
                        }
                    }

                    //Right Column
                    for (int y = 0; y < height; y++)
                    {
                        dataPtr2 = (dataPtr + y * m.widthStep + (width - 1) * nChannels);
                        dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + (width - 1) * nChannels);

                        tempArray[0] = Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + m.widthStep)[0]);
                        tempArray[1] = Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + m.widthStep)[1]);
                        tempArray[2] = Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + m.widthStep)[2]);


                        if (tempArray[0] > 255)
                        {
                            dataPtr2[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr2[0] = 0;
                        }
                        else
                        {
                            dataPtr2[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr2[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr2[1] = 0;
                        }
                        else
                        {
                            dataPtr2[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr2[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr2[2] = 0;
                        }
                        else
                        {
                            dataPtr2[2] = (byte)tempArray[2];
                        }
                    }


                }
            }
        }

        public static void Diferentiation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer(), dataPtr2 = dataPtr;
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer(), dataPtrCopy2 = dataPtrCopy;
                int nChannels = m.nChannels;

                if (nChannels == 3)
                {
                    int[] tempArray = new int[3];
                    for (int y = 0; y < height - 1; y++)
                    {
                        for (int x = 0; x < width - 1; x++)
                        {
                            dataPtr2 = (dataPtr + y * m.widthStep + x * nChannels);
                            dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + x * nChannels);

                            tempArray[0] = Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + nChannels)[0]) + Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + m.widthStep)[0]);
                            tempArray[1] = Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + nChannels)[1]) + Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + m.widthStep)[1]);
                            tempArray[2] = Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + nChannels)[2]) + Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + m.widthStep)[2]);


                            if (tempArray[0] > 255)
                            {
                                dataPtr2[0] = 255;
                            }
                            else if (tempArray[0] < 0)
                            {
                                dataPtr2[0] = 0;
                            }
                            else
                            {
                                dataPtr2[0] = (byte)tempArray[0];
                            }

                            if (tempArray[1] > 255)
                            {
                                dataPtr2[1] = 255;
                            }
                            else if (tempArray[1] < 0)
                            {
                                dataPtr2[1] = 0;
                            }
                            else
                            {
                                dataPtr2[1] = (byte)tempArray[1];
                            }

                            if (tempArray[2] > 255)
                            {
                                dataPtr2[2] = 255;
                            }
                            else if (tempArray[2] < 0)
                            {
                                dataPtr2[2] = 0;
                            }
                            else
                            {
                                dataPtr2[2] = (byte)tempArray[2];
                            }

                        }
                    }

                    //dataPtr = (byte*)m.imageData.ToPointer();
                    //dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    for (int y = 0; y < height - 1; y++)
                    {
                        dataPtr2 = (dataPtr + y * m.widthStep + (width - 1) * nChannels);
                        dataPtrCopy2 = (dataPtrCopy + y * m.widthStep + (width - 1) * nChannels);

                        tempArray[0] = Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + m.widthStep)[0]);
                        tempArray[1] = Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + m.widthStep)[1]);
                        tempArray[2] = Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + m.widthStep)[2]);


                        if (tempArray[0] > 255)
                        {
                            dataPtr2[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr2[0] = 0;
                        }
                        else
                        {
                            dataPtr2[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr2[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr2[1] = 0;
                        }
                        else
                        {
                            dataPtr2[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr2[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr2[2] = 0;
                        }
                        else
                        {
                            dataPtr2[2] = (byte)tempArray[2];
                        }
                    }

                    //dataPtr = (byte*)m.imageData.ToPointer();
                    //dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    for (int x = 0; x < width - 1; x++)
                    {
                        dataPtr2 = (dataPtr + (height - 1) * m.widthStep + x * nChannels);
                        dataPtrCopy2 = (dataPtrCopy + (height - 1) * m.widthStep + x * nChannels);

                        tempArray[0] = Math.Abs(dataPtrCopy2[0] - (dataPtrCopy2 + nChannels)[0]);
                        tempArray[1] = Math.Abs(dataPtrCopy2[1] - (dataPtrCopy2 + nChannels)[1]);
                        tempArray[2] = Math.Abs(dataPtrCopy2[2] - (dataPtrCopy2 + nChannels)[2]);


                        if (tempArray[0] > 255)
                        {
                            dataPtr2[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr2[0] = 0;
                        }
                        else
                        {
                            dataPtr2[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr2[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr2[1] = 0;
                        }
                        else
                        {
                            dataPtr2[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr2[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr2[2] = 0;
                        }
                        else
                        {
                            dataPtr2[2] = (byte)tempArray[2];
                        }
                    }

                    dataPtr2 = (dataPtr + (height - 1) * m.widthStep + (width - 1) * nChannels);
                    dataPtr2[0] = 0;
                    dataPtr2[1] = 0;
                    dataPtr2[2] = 0;
                }
            }

        }

        public static void Median(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            imgCopy.SmoothMedian(3).CopyTo(img);
        }


        public static int[] Histogram_Gray(Emgu.CV.Image<Bgr, byte> img)
        { //dividir 3.0 e arredondar
            unsafe {
                MIplImage m = img.MIplImage;
                int width = img.Width;
                int height = img.Height;
                int padding = m.widthStep - m.nChannels * m.width;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int wStep = m.widthStep;
                int nChan = m.nChannels;

                int[] histogram = new int[256];
                for (int l = 0; l < 256; l++)
                {
                    histogram[l] = 0;
                }
                int gray;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        gray = (int)Math.Round((dataPtr[0] + dataPtr[1] + dataPtr[2]) / 3.0);
                        histogram[gray]++;
                        dataPtr += nChan;
                    }
                    dataPtr += padding;
                }
                return histogram;
            }
        }

        public static int[,] Histogram_RGB(Emgu.CV.Image<Bgr, byte> img)
        { //dividir 3.0 e arredondar
            unsafe
            {
                MIplImage m = img.MIplImage;
                int width = img.Width;
                int height = img.Height;
                int padding = m.widthStep - m.nChannels * m.width;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int wStep = m.widthStep;
                int nChan = m.nChannels;

                int[,] histogram = new int[3, 256];
                for (int s = 0; s < 3; s++)
                {
                    for (int l = 0; l < 256; l++)
                    {
                        histogram[s, l] = 0;
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        histogram[0, dataPtr[0]]++;
                        histogram[1, dataPtr[1]]++;
                        histogram[2, dataPtr[2]]++;
                        dataPtr += nChan;
                    }
                    dataPtr += padding;
                }
                return histogram;
            }
        }

        public static int[,] Histogram_All(Emgu.CV.Image<Bgr, byte> img)
        { //dividir 3.0 e arredondar
            unsafe
            {
                MIplImage m = img.MIplImage;
                int width = img.Width;
                int height = img.Height;
                int padding = m.widthStep - m.nChannels * m.width;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int wStep = m.widthStep;
                int nChan = m.nChannels;

                int[,] histogram = new int[4, 256];
                for (int s = 0; s < 4; s++)
                {
                    for (int l = 0; l < 256; l++)
                    {
                        histogram[s, l] = 0;
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        histogram[0, (int)Math.Round((dataPtr[0] + dataPtr[1] + dataPtr[2]) / 3.0)]++;
                        histogram[1, dataPtr[0]]++;
                        histogram[2, dataPtr[1]]++;
                        histogram[3, dataPtr[2]]++;
                        dataPtr += nChan;
                    }
                    dataPtr += padding;
                }
                return histogram;
            }
        }

        public static void ConvertToBW(Emgu.CV.Image<Bgr, byte> img, int threshold)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                int width = img.Width;
                int height = img.Height;
                int padding = m.widthStep - m.nChannels * m.width;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int wStep = m.widthStep;
                int nChan = m.nChannels;

                if (nChan == 3)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int gray = (int)Math.Round(((int)dataPtr[0] + dataPtr[1] + dataPtr[2]) / 3.0);

                            if (gray > threshold)
                            {
                                dataPtr[0] = 255;
                                dataPtr[1] = 255;
                                dataPtr[2] = 255;
                            }
                            else
                            {
                                dataPtr[0] = 0;
                                dataPtr[1] = 0;
                                dataPtr[2] = 0;
                            }
                            dataPtr += nChan;
                        }
                        dataPtr += padding;
                    }
                }
            }
        }

        public static void ConvertToBW_Otsu(Emgu.CV.Image<Bgr, byte> img)
        {
            int[] histogram = Histogram_Gray(img);

            unsafe
            {
                MIplImage m = img.MIplImage;
                int width = img.Width;
                int height = img.Height;
                int padding = m.widthStep - m.nChannels * m.width;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int wStep = m.widthStep;
                int nChan = m.nChannels;
                double bestVar = 0;
                int bestThreshold=0;

                if (nChan == 3)
                {
                    for (int t = 0; t < 255; t++)
                    {
                        double q1 = 0, q2 = 0, u1 = 0, u2 = 0;

                        for (int i = 0; i <= t; i++)
                        {
                            q1 += histogram[i];
                            u1 += i * histogram[i];
                        }
                        u1 /= q1;

                        for(int i = t+1; i<=255;i++)
                        {
                            q2 += histogram[i];
                            u2 += i * histogram[i];
                        }
                        u2 /= q2;

                        double result = q1 * q2 * (u1 - u2) * (u1 - u2);
                        if(result >= bestVar)
                        {
                            bestThreshold = t;
                            bestVar = result;
                        }
                    }
                    ConvertToBW(img,bestThreshold);
                }
            }
        }
    }
}
