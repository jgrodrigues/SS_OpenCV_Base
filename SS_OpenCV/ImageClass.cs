﻿using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
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

            Pieces_angle = new List<int>(); 
            Pieces_positions = new List<int[]>();
            int[,] matrix = null;
            int numberImages = 0;
            int width = img.Width;
            int height = img.Height;
            int currentImg;
            List<Image<Bgr, byte>> pieces = new List<Image<Bgr, byte>>();

            if (level == 1) {             
                matrix = imageFinder(dummyImg, Pieces_positions, out numberImages);
                Pieces_positions = getPositions(matrix,height,width,numberImages,Pieces_positions,img);

                // for(currentImg = 0; currentImg<numberImages; currentImg++) {
                //     Pieces_angle.Add(0);
                //     pieces.Add(getNormalPiece(img, Pieces_positions[currentImg][0], Pieces_positions[currentImg][1], Pieces_positions[currentImg][2], Pieces_positions[currentImg][3]));
                // }  

                // if(numberImages == 1){
                //     dummyImg = pieces[0];
                // } else {
                //     dummyImg = joinPiecesLevel1(pieces[0], pieces[1]);
                // }

                               
            
            } else if (level == 2) {
                matrix = imageFinder(dummyImg,Pieces_positions, out numberImages);
                Pieces_positions = getPositions(matrix,height,width,numberImages,Pieces_positions,img);
                // int[,] superiorCorners = findRotations(img,Pieces_angle,matrix,numberImages);
                // int[,] bottomCorners = getBottomRightCorners(img,matrix, numberImages);

                // for(currentImg=0; currentImg<numberImages; currentImg++) {
                //     if(Pieces_angle[currentImg] == 0){
                //         pieces.Add(getNormalPiece(img, superiorCorners[currentImg,0], superiorCorners[currentImg,1], bottomCorners[currentImg,0], bottomCorners[currentImg,1]));
                //     } else {
                //         //Acabar aqui
                //         bottomCorners = getBottomRightCorners(img,matrix,numberImages);
                //         pieces.Add(getRotatedPiece());
                //     }
                // }

                // dummyImg = joinPiecesLevel1(pieces[0], pieces[1]);
            }

            else
            {

            }

            return dummyImg;
        }

        /// </summary>
        /// Function that returns a piece of the puzzle that is not rotated
        /// </summary>
        /// <param name="img">The whole puzzle</param>
        /// <param name="xTopLeft">X coordinate in the img of the upper left corner</param>
        /// <param name="yTopLeft">Y coordinate in the img of the upper left corner</param>
        /// <param name="xBottomRight">X coordinate in the img of the bottom right corner</param>
        /// <param name="yBottomRight">Y coordinate in the img of the bottom right corner</param>
        private static Image<Bgr, byte> getNormalPiece(Image<Bgr, byte> img, int xTopLeft, int yTopLeft, int xBottomRight, int yBottomRight){
            unsafe {
                int heightPiece = yBottomRight - yTopLeft;
                int widthPiece = xBottomRight - xTopLeft;
                Image<Bgr, byte> piece = new Image<Bgr, byte>(widthPiece, heightPiece);
                MIplImage mPiece = piece.MIplImage;
                MIplImage mImg = img.MIplImage;
                byte* dataPtrPiece = (byte*)mPiece.imageData.ToPointer();
                byte* dataPtrImg = (byte*)mImg.imageData.ToPointer();
                int nChan = mImg.nChannels;
                int widthStepImg = mImg.widthStep;
                int widthStepPiece = mPiece.widthStep;
                int paddingPiece = widthStepPiece - nChan * widthPiece;

                dataPtrImg += nChan * xTopLeft + widthStepImg * yTopLeft;

                for (int y=0; y<heightPiece; y++) {
                    for (int x=0; x<widthPiece; x++) {
                        dataPtrPiece[0] = dataPtrImg[0];
                        dataPtrPiece[1] = dataPtrImg[1];
                        dataPtrPiece[2] = dataPtrImg[2];

                        dataPtrPiece += nChan;
                        dataPtrImg += nChan;
                    }

                    dataPtrPiece += paddingPiece;
                    dataPtrImg += widthStepImg - (widthPiece*nChan);
                }
                return piece;
            }
        }

        //Function that is going to be used to get a rotated Piece
        private static Image<Bgr, byte> getRotatedPiece(){
            return null;
        }

        /// </summary>
        /// Function that finds the rotation of each image, and returns the coordinates of the both upper corners.
        /// </summary>
        /// <param name="img">The puzzle eunsolved</param>
        /// <param name="Pieces_angle">The list to be filled with each piece's angle</param>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static int[,] findRotations(Image<Bgr,byte> img, List<int> Pieces_angle, int[,] matrix, int numberImages) {
            int x, y;
            int height = img.Height;
            int width = img.Width;

            // Index 0 - x of upper left corner
            // Index 1 - y of upper left corner
            // Index 2 - x of upper right corner
            // Index 3 - y of upper right corner
            int[,] superiorCorners = new int[numberImages, 4];

            //Finds superior left corner of each peace
            int currentLabel = 0;

            for (y=0; y<height && numberImages!=currentLabel; y++) {
                for (x=0; x<width && numberImages!=currentLabel; x++) {
                    if (matrix[x, y] > currentLabel && matrix[x,y] != 0) {
                        superiorCorners[currentLabel,0] = x;
                        superiorCorners[currentLabel,1] = y;
                        currentLabel++;
                    }
                }
            }

            //Finds superior right corner of each piece
            int[] checkedLabels = new int[numberImages];
            int checkCount = 0;
            currentLabel=0;

            for (y=0; y<height && checkCount!=numberImages; y++) {
                for (x=width-1; x<=0 && checkCount!=numberImages; x++) {
                    if (matrix[x, y] != currentLabel && matrix[x,y] != 0) {
                        currentLabel = matrix[x,y];
                        
                        if(checkedLabels[currentLabel] == 0){
                            superiorCorners[currentLabel,2] = x;
                            superiorCorners[currentLabel,3] = y;
                            checkedLabels[currentLabel] = 1;
                            checkCount++;
                        }
                    }
                }
            }

            //Gets the angle of rotation of each image
            int angle;
            int xTopLeft = 0;
            int yTopLeft = 0;
            int xTopRight = 0;
            int yTopRight = 0;

            for(int i = 0;i<numberImages;i++) {
                xTopLeft = superiorCorners[i,0];
                yTopLeft = superiorCorners[i,1];
                xTopRight = superiorCorners[i,2];
                yTopRight = superiorCorners[i,3];

                if(yTopLeft == yTopRight) {
                    Pieces_angle.Add(0);
                } else {
                    angle = (int)Math.Round(Math.Atan((yTopLeft-yTopRight)*1.0/(xTopRight-xTopLeft)));
                    Pieces_angle.Add(angle);
                }
            }

            return superiorCorners;
        }

        /// </summary>
        /// Function that gives the coordinates of the bottom right corner, when the image is rotated and when is not.
        /// </summary>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static int[,] getBottomRightCorners(Image<Bgr,byte> img, int[,] matrix, int numberImages) {
            int[,] bottomCorners = new int[numberImages, 2];
            int height = img.Height;
            int width = img.Width;
            int x,y;
            int currentLabel = 0;
            int[] seen = new int[numberImages];
            int numberSeen = 0;

            for (x=width-1; x>=0 && numberImages != numberSeen; x--) {
                for (y=height-1; y >= 0 && numberImages != numberSeen ; y--) {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && seen[label-1] == 0) {
                        bottomCorners[label-1,0] = x; 
                        bottomCorners[label-1,1] = y;
                        currentLabel = label;
                        seen[label-1] = 1;
                        numberSeen++;
                    }
                }
            }
            return bottomCorners;
        }

        /// </summary>
        /// Function that gives the coordinates of the bottom left corner, when the image is rotated and when is not.
        /// </summary>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static int[,] getBottomLeftCorners(Image<Bgr,byte> img, int[,] matrix, int numberImages) {
            int[,] bottomCorners = new int[numberImages, 2];
            int height = img.Height;
            int width = img.Width;
            int x,y;
            int currentLabel = 0;
            int[] seen = new int[numberImages];
            int numberSeen = 0;

            for (y = height - 1; y >= 0 && numberImages != numberSeen ; y--) {
                for (x = 0; x < width && numberImages != numberSeen; x++) {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && seen[label-1] == 0) {
                        bottomCorners[label-1,0] = x; 
                        bottomCorners[label-1,1] = y;
                        currentLabel = label;
                        seen[label-1] = 1;
                        numberSeen++;
                    }
                }
            }
            return bottomCorners;
        }

        /// </summary>
        /// Function that gives the coordinates of the upper left corner.
        /// </summary>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static int[,] getUpperLeftCorners(Image<Bgr,byte> img, int[,] matrix, int numberImages) {
            int[,] upperCorners = new int[numberImages, 2];
            int height = img.Height;
            int width = img.Width;
            int x,y;
            int currentLabel = 0;
            int[] seen = new int[numberImages];
            int numberSeen = 0;

            for (x = 0; x < width && numberImages != numberSeen; x++) {
                for (y = 0; y < height && numberImages != numberSeen; y++) {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && seen[label-1] == 0) {
                        upperCorners[label-1,0] = x; //xStart
                        upperCorners[label-1,1] = y; //yStart
                        currentLabel = label;
                        seen[label-1] = 1;
                        numberSeen++;
                    }
                }
            }
            return upperCorners;
        }

        /// </summary>
        /// Function that gives the coordinates of the upper right corner.
        /// </summary>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static int[,] getUpperRightCorners(Image<Bgr,byte> img, int[,] matrix, int numberImages) {
            int[,] upperCorners = new int[numberImages, 2];
            int height = img.Height;
            int width = img.Width;
            int x,y;
            int currentLabel = 0;
            int[] seen = new int[numberImages];
            int numberSeen = 0;
            
            for (y=0; y<height && numberImages != numberSeen; y++) {
                for (x = width-1; x>=0 && numberImages != numberSeen; x--) {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && seen[label-1] == 0) {
                        upperCorners[label-1,0] = x; //xStart
                        upperCorners[label-1,1] = y; //yStart
                        currentLabel = label;
                        seen[label-1] = 1;
                        numberSeen++;
                    }
                }
            }
            return upperCorners;
        }

        /// </summary>
        /// Function that, given an array of ints, checks wich index has the smallest number.
        /// </summary>
        /// <param name="differences">the int array, each index represents the difference between two sides of two pictures.</param>
        //Index0 is the difference between left of image1 and right of image2
        //Index1 is the difference between right of image1 and left of image2
        //Index2 is the difference between top of image1 and bottom of image2
        //Index3 is the difference between bottom of image1 and top of image2 
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

        /// </summary>
        /// Function that resolves a level 1 puzzle, givem the pieces of the puzzle
        /// </summary>
        /// <param name="piece1">One piece to be joint</param>
        /// <param name="piece2">Another piece to be joint</param>
        public static Image<Bgr, byte> joinPiecesLevel1 (Image<Bgr, byte> piece1, Image<Bgr, byte> piece2) {
            Image<Bgr, byte> dummyImg;
            MIplImage mPiece1 = piece1.MIplImage;
            MIplImage mPiece2 = piece2.MIplImage;
            int widthPiece1 = mPiece1.width;
            int heightPiece1 = mPiece1.height;
            int widthPiece2 = mPiece2.width;
            int heightPiece2 = mPiece2.height;
            int[] differencesLeftRight = null;
            int[] differencesTopBottom = null;   

            //If the two pieces have the exact same measures
            if(widthPiece1 == widthPiece2 && heightPiece1 == heightPiece2){
                //Index0 is the difference between left of image1 and right of image2
                //Index1 is the difference between right of image1 and left of image2
                //Index2 is the difference between top of image1 and bottom of image2
                //Index3 is the difference between bottom of image1 and top of image2                
                int[] differences = new int[4];
                differencesLeftRight =  checkLeftRight(piece1, piece2);
                differencesTopBottom = checkTopBottom(piece1, piece2);
                differences[0] = differencesLeftRight[0];
                differences[1] = differencesLeftRight[1];
                differences[2] = differencesTopBottom[0];
                differences[3] = differencesTopBottom[1];
                    
                int leastDiff = leastDifference(differences);

                //if the joint is to be made between right and left sides
                if(leastDiff<2){

                    if(leastDiff == 0){
                        dummyImg = joinLeftRight(piece2, piece1);
                    } else {
                        dummyImg = joinLeftRight(piece1, piece2); 
                }

                //If the joint is to be made between top and bottom sides
                } else {

                    if(leastDiff == 2){
                        dummyImg = joinTopBottom(piece2, piece1);
                    } else {
                        dummyImg = joinTopBottom(piece1, piece2);                    
                    }
                }

            //If the two pieces only have the same width                                         
            } else if (widthPiece1 == widthPiece2) {
                //Index0 is the difference between top of image1 and bottom of image2
                //Index1 is the difference between bottom of image1 and top of image2                            
                differencesTopBottom = checkTopBottom(piece1, piece2);

                if(differencesTopBottom[0] < differencesTopBottom[1]){
                    dummyImg = joinTopBottom(piece2, piece1);
                } else {
                    dummyImg = joinTopBottom(piece1, piece2);                    
                }
                    
            //If the two pieces only have the samee height
            } else {
                //Index0 is the difference between left of image1 and right of image2
                //Index1 is the difference between right of image1 and left of image2                
                differencesLeftRight = checkLeftRight(piece1, piece2);
                    
                if(differencesLeftRight[0] < differencesLeftRight[1]){
                    dummyImg = joinLeftRight(piece1, piece2);
                } else{
                    dummyImg = joinLeftRight(piece2, piece1);
                }                              
            }

            return dummyImg;                
        }

        /// </summary>
        /// Function that joins two pieces, one on the top and other one on the bottom, and returns the result of the joint
        /// </summary>
        /// <param name="piece1">The piece to be on the top</param>
        /// <param name="piece2">The piece to be on the bottom</param>
        private static Image<Bgr, byte> joinTopBottom(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2){
            unsafe{
                MIplImage mPiece1 = piece1.MIplImage;
                MIplImage mPiece2 = piece2.MIplImage;
                int nChan = mPiece1.nChannels;
                int totalWidth = mPiece1.width;
                int heightPiece1 = mPiece1.height;
                int heightPiece2 = mPiece2.height;
                int totalHeight = heightPiece1 + heightPiece2;
                int widthStepPiece1 = mPiece1.widthStep;
                int widthStepPiece2 = mPiece2.widthStep;
                int paddingPiece1 = widthStepPiece1 - nChan * totalWidth;
                int paddingPiece2 = widthStepPiece2 - nChan * totalWidth;
                int x, y;

                Image<Bgr, byte> dummyImg = new Image<Bgr, byte>(totalWidth, totalHeight);
                MIplImage mDummy = dummyImg.MIplImage;
                int heightPiece = mDummy.height;
                byte* dataPtrDummy = (byte*)mDummy.imageData.ToPointer();
                int widthStepDummy = mDummy.widthStep;
                int paddingDummy = widthStepDummy - nChan * totalWidth;

                //Pointer of the top piece
                byte* dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                //Pointer of the bottom piece
                byte* dataPtr2 = (byte*)mPiece2.imageData.ToPointer();
            
                //Copies to dummyImg the top piece
                for(y=0; y<heightPiece1; y++){
                    for(x=0; x<totalWidth; x++){
                        dataPtrDummy[0] = dataPtr1[0];
                        dataPtrDummy[1] = dataPtr1[1];
                        dataPtrDummy[2] = dataPtr1[2];
                        dataPtr1 += nChan;
                        dataPtrDummy += nChan;
                    }

                    dataPtr1 += paddingPiece1;
                    dataPtrDummy += paddingDummy;
                }

                //Copies to dummyImg the bottomPiece
                for(y=heightPiece1; y<totalHeight; y++){
                    for(x=0; x<totalWidth; x++){
                        dataPtrDummy[0] = dataPtr2[0];
                        dataPtrDummy[1] = dataPtr2[1];
                        dataPtrDummy[2] = dataPtr2[2];
                        dataPtr2 += nChan;
                        dataPtrDummy += nChan;
                    }

                    dataPtr2 += paddingPiece2;
                    dataPtrDummy += paddingDummy;
                }

                return dummyImg;
            }
        }

        /// </summary>
        /// Function that joins two pieces, one on the left and other one on the right, and returns the result of the joint
        /// </summary>
        /// <param name="piece1">The piece to be on the left</param>
        /// <param name="piece2">The piece to be on the right</param>
        private static Image<Bgr, byte> joinLeftRight(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2){
            unsafe{
                MIplImage mPiece1 = piece1.MIplImage;
                MIplImage mPiece2 = piece2.MIplImage;
                int nChan = mPiece1.nChannels;
                int totalHeight = mPiece1.height;
                int widthPiece1 = mPiece1.width;
                int widthPiece2 = mPiece2.width;
                int totalWidth = widthPiece1 + widthPiece2;
                int widthStepPiece1 = mPiece1.widthStep;
                int widthStepPiece2 = mPiece2.widthStep;
                int paddingPiece1 = widthStepPiece1 - nChan * widthPiece1;
                int paddingPiece2 = widthStepPiece2 - nChan * widthPiece2;
                int x, y;

                Image<Bgr, byte> dummyImg = new Image<Bgr, byte>(totalWidth, totalHeight);
                MIplImage mDummy = dummyImg.MIplImage;
                byte* dataPtrDummy = (byte*)mDummy.imageData.ToPointer();
                int widthStepDummy = mDummy.widthStep;
                int paddingDummy = widthStepDummy - nChan * totalWidth;

                //Pointer of the left piece
                byte* dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                //Pointer of the right piece
                byte* dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                for(y=0; y<totalHeight; y++){
                    //Copies to dummyImg the left piece
                    for(x=0; x<widthPiece1; x++){
                        dataPtrDummy[0] = dataPtr1[0];
                        dataPtrDummy[1] = dataPtr1[1];
                        dataPtrDummy[2] = dataPtr1[2];
                        dataPtr1 += nChan;
                        dataPtrDummy += nChan;
                    }

                    //Copies to dummyImg the right piece
                    for(x=widthPiece1; x<totalWidth; x++){
                        dataPtrDummy[0] = dataPtr2[0];
                        dataPtrDummy[1] = dataPtr2[1];
                        dataPtrDummy[2] = dataPtr2[2];
                        dataPtr2 += nChan;
                        dataPtrDummy += nChan;
                    }

                    dataPtr1 += paddingPiece1;
                    dataPtr2 += paddingPiece2;
                    dataPtrDummy += paddingDummy;
                }

                return dummyImg;
            }
        }                   
        
        public static int[] checkLeftRight(Image<Bgr,byte> piece1, Image<Bgr,byte> piece2) {
            unsafe {
                MIplImage mPiece1 = piece1.MIplImage;
                MIplImage mPiece2 = piece2.MIplImage;

                int[] differencesLeftRight = new int[2];
                int height = mPiece1.height;
                int widthPiece1 = mPiece1.width;
                int widthPiece2 = mPiece2.width;
                int widthStepPiece1 = mPiece1.widthStep;
                int widthStepPiece2 = mPiece2.widthStep;
                int nChan = mPiece1.nChannels;
                byte* dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                byte* dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                //Compare left of image1 to right of image2
                dataPtr2 += nChan * widthPiece2;
                for(int y=0;y<height;y++) {
                    differencesLeftRight[0] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += widthStepPiece1;
                    dataPtr2 += widthStepPiece2;
                }

                dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                //Compare right of image1 to left of image2
                dataPtr1 += nChan * widthPiece1;
                for(int y=0; y<height; y++) {
                    differencesLeftRight[1] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += widthStepPiece1;
                    dataPtr2 += widthStepPiece2;
                }
                return differencesLeftRight;
            }
        }

        public static int[] checkTopBottom(Image<Bgr,byte> piece1, Image<Bgr,byte> piece2) {
            unsafe {
                MIplImage mPiece1 = piece1.MIplImage;
                MIplImage mPiece2 = piece2.MIplImage;

                int[] differencesTopBottom = new int[2];
                int width = mPiece1.width;
                int heightPiece1 = mPiece1.height;
                int heightPiece2 = mPiece2.height;
                int widthStepPiece1 = mPiece1.widthStep;
                int widthStepPiece2 = mPiece2.widthStep;
                int nChan = mPiece1.nChannels;
                byte* dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                byte* dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                //Compare top of image1 to bottom of image2
                //dataPtr2 += widthStepPiece2 * heightPiece2;
                for(int x=0; x<width; x++) {
                    differencesTopBottom[0] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += nChan;
                    dataPtr2 += nChan;
                }

                dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                //Compare bottom of image1 to top of image2
                //dataPtr1 += widthStepPiece1 * heightPiece1;
                for(int x=0; x<width; x++) {
                    differencesTopBottom[1] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += nChan;
                    dataPtr2 += nChan;
                }

                return differencesTopBottom;
            }
        }

        public static int[,] imageFinder(Image<Bgr, byte> img, List<int[]> Pieces_positions, out int numberImages)
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
                numberImages = 0;

                int nChan = mStart.nChannels;
                int padding = mStart.widthStep - mStart.nChannels * mStart.width;
                int widthStep = mStart.widthStep;
                int x, y;
                int left, bottom, right, top, label, topLeft, topRight, botLeft, botRight;
                
                //Algoritmo dos componentes ligados
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
                                
                                //Ateração
                                topLeft = matrix[x-1, y-1];
                                topRight = matrix[x+1, y-1];
                                botLeft = matrix[x-1, y+1];
                                botRight = matrix[x+1, y+1];

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
                                if (botLeft != 0 && botLeft < label)
                                {
                                    label = botLeft;
                                }
                                if (botRight != 0 && botRight < label)
                                {
                                    label = botRight;
                                }
                                if (topLeft != 0 && topLeft < label)
                                {
                                    label = topLeft;
                                }
                                if (topRight != 0 && topRight < label)
                                {
                                    label = topRight;
                                }
                                if (label == width * height)
                                {
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

                //Top Margin
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan;

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
                x = 0;                     
                for(y = 1; y < height-1; y++) {

                    if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
                        top = matrix[x, y-1];
                        right = matrix[x+1, y];
                        label = width * height;                    

                        if (top != 0 && top < label)
                        {
                            label = top;
                        }

                        if (right != 0 && right < label)
                        {
                            label = right;
                        }   

                        matrix[x, y] = label;  
                    }                                                                                                                                         
                }     

                //Right Margin
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep + nChan * (width - 1);
                x = width-1;                                        
                for(y = 1; y < height-1; y++) {    

                    if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
                        left = matrix[x - 1,y];
                        top = matrix[x, y-1];
                        label = width * height;                    

                        if (left != 0 && left < label)
                        {
                            label = left;
                        }

                        if (top != 0 && top < label)
                        {
                            label = top;
                        }

                        matrix[x, y] = label;  
                    }
                }

                //Superior Right Corner
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan * (width-1);
                x = width-1;
                if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
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

                    matrix[x,0] = label;
                }
                

                //Inferior Left Corner
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height-1);
                y = height-1;
                if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
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

                    matrix[0,y] = label;
                }

                //Inferior Right Corner 
                dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height-1) + nChan * (height-1);    
                x = width-1;
                y = height-1;
                if (dataPtr[0] != bBack || dataPtr[1] != gBack || dataPtr[2] != rBack){
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

                    matrix[x,y] = label;
                }      

                dataPtr = (byte*)mStart.imageData.ToPointer();
                List<int> labelsSeen = new List<int>();
                numberImages = 0;
                
                for (y = 0; y < height; y++) {
                    for (x = 0; x < width; x++) {
                        label = matrix[x,y];
                        if(label!=0 && !labelsSeen.Contains(label)) {
                            Pieces_positions.Add(new int[4]);
                            numberImages++;
                            labelsSeen.Add(label);
                        }
                    }
                }                        

                return matrix;
            }
        }
        

        private static List<int[]> getPositions(int[,] matrix, int height, int width, int numberImages,List<int[]> Pieces_positions, Image<Bgr,byte> img){

            int[,] upperRightCorner = getUpperRightCorners(img,matrix,numberImages); //TODO tirar
            int[,] upperLeftCorner = getUpperLeftCorners(img,matrix,numberImages);
            int[,] lowerLeftCorner = getBottomLeftCorners(img,matrix,numberImages); //TODO tira
            int[,] lowerRightCorner = getBottomRightCorners(img,matrix,numberImages);
            int[] currPiecePositions = new int[4];
            for(int i = 0;i<numberImages;i++) {
                currPiecePositions[0] = upperLeftCorner[i,0];
                currPiecePositions[1] = upperLeftCorner[i,1];
                currPiecePositions[2] = lowerRightCorner[i,0];
                currPiecePositions[3] = lowerRightCorner[i,1];
                Pieces_positions.Add(currPiecePositions);
            }

            return Pieces_positions;
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
