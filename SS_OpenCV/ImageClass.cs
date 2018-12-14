using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
namespace SS_OpenCV
{
    static class ImageClass
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
            List<double> Pieces_angle_precise = new List<double>();
            Pieces_positions = new List<int[]>();
            int[,] matrix = null;
            int numberImages = 0;
            int width = img.Width;
            int height = img.Height;
            int currentImg;
            List<Image<Bgr, byte>> pieces = new List<Image<Bgr, byte>>();
            List<int> piecesLabels = new List<int>();

            if (level == 1)
            {
                matrix = imageFinder(dummyImg, out numberImages, piecesLabels);
                Pieces_positions = getPositions(matrix, height, width, numberImages, img, piecesLabels);

                for (currentImg = 0; currentImg < numberImages; currentImg++)
                {
                    Pieces_angle_precise.Add(0.0f);
                    pieces.Add(getNormalPiece(img, Pieces_positions[currentImg][0], Pieces_positions[currentImg][1], Pieces_positions[currentImg][2], Pieces_positions[currentImg][3]));
                }

                if (numberImages == 1)
                {
                    dummyImg = pieces[0];
                }
                else
                {
                    dummyImg = joinPiecesLevel1(pieces[0], pieces[1]);
                }

            }
            else if (level == 2)
            {
                matrix = imageFinder(dummyImg, out numberImages, piecesLabels);
                Pieces_positions = getPositions(matrix, height, width, numberImages, img, piecesLabels);
                List<int[]> upperRightCorners = getUpperRightCorners(img, matrix, numberImages, piecesLabels);
                List<int[]> bottomLeftCorners = getBottomLeftCorners(img, matrix, numberImages, piecesLabels);
                List<int[]> upperLeftCorners = getUpperLeftCorners(img, matrix, numberImages, piecesLabels);
                Pieces_angle_precise = findRotations(img, matrix, numberImages, upperRightCorners, upperLeftCorners);

                for (currentImg = 0; currentImg < numberImages; currentImg++)
                {

                    if (Pieces_angle_precise[currentImg] == 0)
                    {
                        pieces.Add(getNormalPiece(img, Pieces_positions[currentImg][0], Pieces_positions[currentImg][1], Pieces_positions[currentImg][2], Pieces_positions[currentImg][3]));
                    }
                    else
                    {
                        pieces.Add(getRotatedPiece(img, upperRightCorners[currentImg], bottomLeftCorners[currentImg], Pieces_positions[currentImg], Pieces_angle_precise[currentImg],piecesLabels));
                    }
                }
                foreach (double angle in Pieces_angle_precise)
                {
                    Pieces_angle.Add((int)angle);
                }
                //dummyImg = pieces[1];
                dummyImg = joinPiecesLevel1(pieces[0], pieces[1]);
            }
            else
            {
                matrix = imageFinder(dummyImg, out numberImages,piecesLabels);
                Pieces_positions = getPositions(matrix, height, width, numberImages, img, piecesLabels);
                List<int[]> upperRightCorners = getUpperRightCorners(img, matrix, numberImages, piecesLabels);
                List<int[]> bottomLeftCorners = getBottomLeftCorners(img, matrix, numberImages, piecesLabels);
                List<int[]> upperLeftCorners = getUpperLeftCorners(img, matrix, numberImages, piecesLabels);
                Pieces_angle_precise = findRotations(img, matrix, numberImages, upperRightCorners, upperLeftCorners);

                for (currentImg = 0; currentImg < numberImages; currentImg++)
                {

                    if (Pieces_angle_precise[currentImg] == 0)
                    {
                        pieces.Add(getNormalPiece(img, Pieces_positions[currentImg][0], Pieces_positions[currentImg][1], Pieces_positions[currentImg][2], Pieces_positions[currentImg][3]));
                    }
                    else
                    {
                        pieces.Add(getRotatedPiece(img, upperRightCorners[currentImg], bottomLeftCorners[currentImg], Pieces_positions[currentImg], Pieces_angle_precise[currentImg], piecesLabels));
                    }
                }
                foreach (double angle in Pieces_angle_precise)
                {
                    Pieces_angle.Add((int)angle);
                }

                dummyImg = joinPiecesLevel3(pieces);
            }

            foreach (int angle in Pieces_angle_precise)
            {
                Pieces_angle.Add(angle);
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
        private static Image<Bgr, byte> getNormalPiece(Image<Bgr, byte> img, int xTopLeft, int yTopLeft, int xBottomRight, int yBottomRight)
        {
            unsafe
            {
                int heightPiece = yBottomRight - yTopLeft + 1;
                int widthPiece = xBottomRight - xTopLeft + 1;
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

                for (int y = 0; y < heightPiece; y++)
                {
                    for (int x = 0; x < widthPiece; x++)
                    {
                        dataPtrPiece[0] = dataPtrImg[0];
                        dataPtrPiece[1] = dataPtrImg[1];
                        dataPtrPiece[2] = dataPtrImg[2];

                        dataPtrPiece += nChan;
                        dataPtrImg += nChan;
                    }

                    dataPtrPiece += paddingPiece;
                    dataPtrImg += widthStepImg - (widthPiece * nChan);
                }
                return piece;
            }
        }

        private static Image<Bgr, byte> getPieceAfterRotation(Image<Bgr, byte> img, int correctWidth, int correctHeight)
        {
            unsafe
            {
                int oldHeightPiece = img.Height;
                int oldWidthPiece = img.Width;
                MIplImage mImg = img.MIplImage;
                
                int nChan = mImg.nChannels;
                int widthStepImg = mImg.widthStep;

                Image<Bgr, byte> piece = null;
                Image<Bgr, byte> dummy = img.Copy();

                List<int> linesToRemove = new List<int>();
                List<int> columnsToRemove = new List<int>();

                if (oldWidthPiece > correctWidth)
                {
                    columnsToRemove =  getColumnsToRemove(dummy, oldWidthPiece - correctWidth);
                }
                if (oldHeightPiece > correctHeight)
                {
                    linesToRemove = getLinesToRemove(dummy, oldHeightPiece - correctHeight);
                }
                int newWidth = oldWidthPiece-columnsToRemove.Count;
                int newHeight = oldHeightPiece-linesToRemove.Count;

                piece = new Image<Bgr, byte>(newWidth, newHeight);
                MIplImage mPiece = piece.MIplImage;
                byte* dataPtrPiece = (byte*)mPiece.imageData.ToPointer();
                byte* dataPtrImg = (byte*)mImg.imageData.ToPointer();
                int widthStepPiece = mPiece.widthStep;
                int paddingPiece = widthStepPiece - nChan * newWidth;
                int paddingImg = widthStepImg - nChan * oldWidthPiece;

                for (int y = 0; y < oldHeightPiece; y++)
                {
                    if (!linesToRemove.Contains(y))
                    {
                        for (int x = 0; x < oldWidthPiece; x++)
                        {
                            if (!columnsToRemove.Contains(x))
                                {
                                dataPtrPiece[0] = dataPtrImg[0];
                                dataPtrPiece[1] = dataPtrImg[1];
                                dataPtrPiece[2] = dataPtrImg[2];
                                dataPtrPiece += nChan;
                            }
                            dataPtrImg += nChan;  
                        }
                        dataPtrPiece += paddingPiece;
                        dataPtrImg += paddingImg;
                    } else {
                        dataPtrImg += widthStepImg;
                    }
                }
                return piece;
            }
        }

        private static List<int> getColumnsToRemove(Image<Bgr, byte> img, int nToRemove)
        {
            unsafe
            {
                int heightPiece = img.Height;
                int widthPiece = img.Width;
                MIplImage mImg = img.MIplImage;
                int nChan = mImg.nChannels;
                int widthStepImg = mImg.widthStep;
                List<int> columnsToRemove = new List<int>();
                int nLeftColumnsRemoved = 0;
                int nRightColumnsRemoved = 0;

                for(int n = 0; n<nToRemove; n++) {
                    int differenceLeftSide = 0;
                    int differenceRightSide = 0;
                    byte* dataPtrImg = (byte*)mImg.imageData.ToPointer() + nLeftColumnsRemoved*nChan;
                    byte* dataPtrImg2 = (byte*)mImg.imageData.ToPointer() + nChan + nLeftColumnsRemoved*nChan;

                    for (int y = 0; y < heightPiece; y++)
                    {
                        differenceLeftSide += Math.Abs(dataPtrImg[0] - dataPtrImg2[0]) +
                            Math.Abs(dataPtrImg[1] - dataPtrImg2[1]) +
                            Math.Abs(dataPtrImg[2] - dataPtrImg2[2]);

                        dataPtrImg += widthStepImg;
                        dataPtrImg2 += widthStepImg;
                    }

                    //calculate difference on right side
                    dataPtrImg =(byte*)mImg.imageData.ToPointer()+ nChan*(widthPiece-1) - nRightColumnsRemoved*nChan;
                    dataPtrImg2 =(byte*)mImg.imageData.ToPointer()+ nChan*(widthPiece-1) - nChan - nRightColumnsRemoved*nChan;

                    for (int y = 0; y < heightPiece; y++)
                    {
                        differenceRightSide += Math.Abs(dataPtrImg[0] - dataPtrImg2[0]) +
                            Math.Abs(dataPtrImg[1] - dataPtrImg2[1]) +
                            Math.Abs(dataPtrImg[2] - dataPtrImg2[2]);

                        dataPtrImg += widthStepImg;
                        dataPtrImg2 += widthStepImg;
                    }

                    if(differenceLeftSide > differenceRightSide) {
                        columnsToRemove.Add(nLeftColumnsRemoved++);
                    } else {
                        columnsToRemove.Add(widthPiece-1-nRightColumnsRemoved++);
                    }
                }
                return columnsToRemove;
            }
        }

        private static List<int> getLinesToRemove(Image<Bgr, byte> img, int nToRemove)
        {
            unsafe
            {
                int heightPiece = img.Height;
                int widthPiece = img.Width;
                MIplImage mImg = img.MIplImage;
                int nChan = mImg.nChannels;
                int widthStepImg = mImg.widthStep;
                List<int> linesToRemove = new List<int>();
                int nTopLinesRemoved = 0;
                int nBottomLinesRemoved = 0;

                for(int n = 0;n<nToRemove;n++) {
                    byte* dataPtrImg = (byte*)mImg.imageData.ToPointer() + widthStepImg*nTopLinesRemoved;
                    byte* dataPtrImg2 = (byte*)mImg.imageData.ToPointer() + widthStepImg + widthStepImg*nTopLinesRemoved;

                    int differenceTopSide = 0;
                    int differenceBottomSide = 0;

                    //calculate difference on top side
                    for (int x = 0; x < widthPiece; x++)
                    {
                        differenceTopSide += Math.Abs(dataPtrImg[0] - dataPtrImg2[0]) +
                            Math.Abs(dataPtrImg[1] - dataPtrImg2[1]) +
                            Math.Abs(dataPtrImg[2] - dataPtrImg2[2]);

                        dataPtrImg += nChan;
                        dataPtrImg2 += nChan;
                    }

                    //calculate difference on bottom side
                    dataPtrImg = (byte*)mImg.imageData.ToPointer()+widthStepImg*(heightPiece-1) - widthStepImg*nBottomLinesRemoved;
                    dataPtrImg2 = (byte*)mImg.imageData.ToPointer()+widthStepImg*(heightPiece-1) - widthStepImg - widthStepImg*nBottomLinesRemoved;

                    for (int x = 0; x < widthPiece; x++)
                    {
                        differenceBottomSide += Math.Abs(dataPtrImg[0] - dataPtrImg2[0]) +
                            Math.Abs(dataPtrImg[1] - dataPtrImg2[1]) +
                            Math.Abs(dataPtrImg[2] - dataPtrImg2[2]);

                        dataPtrImg += nChan;
                        dataPtrImg2 += nChan;
                    }
                    if(differenceTopSide > differenceBottomSide) {
                        //Remover Top line
                        linesToRemove.Add(nTopLinesRemoved++);
                    } else {
                        //Remover Bottom Line
                        linesToRemove.Add(heightPiece-1-nBottomLinesRemoved++);
                    }
                }
                return linesToRemove;
            }
        }

        //Function that is going to be used to get a rotated Piece
        private static Image<Bgr, byte> getRotatedPiece(Image<Bgr, byte> img, int[] topRight, int[] bottomLeft, int[] Piece_positions, double angle, List<int> piecesLabels)
        {
            unsafe
            {
                int heightPiece = bottomLeft[1] - topRight[1] + 1;
                int widthPiece = Piece_positions[2] - Piece_positions[0] + 1;
                Image<Bgr, byte> piece = new Image<Bgr, byte>(widthPiece, heightPiece);
                MIplImage mPiece = piece.MIplImage;
                MIplImage mImg = img.MIplImage;
                byte* dataPtrPiece = (byte*)mPiece.imageData.ToPointer();
                byte* dataPtrImg = (byte*)mImg.imageData.ToPointer();
                int nChan = mImg.nChannels;
                int widthStepImg = mImg.widthStep;
                int widthStepPiece = mPiece.widthStep;
                int paddingPiece = widthStepPiece - nChan * widthPiece;
                int x, y;
                byte[] background = new byte[3];

                //int resultingWidth = (int)Math.Ceiling((Piece_positions[2] - bottomLeft[0])/Math.Cos(angle));
                int correctWidth = (int)Math.Round(Math.Sqrt(Math.Pow(Piece_positions[2] - bottomLeft[0], 2.0f) + Math.Pow(Piece_positions[3] - bottomLeft[1], 2.0f)))+1;
                int correctHeight = (int)Math.Round(Math.Sqrt(Math.Pow(bottomLeft[0] - Piece_positions[0], 2.0f) + Math.Pow(bottomLeft[1] - Piece_positions[1], 2.0f))+1);

                background[0] = dataPtrImg[0];
                background[1] = dataPtrImg[1];
                background[2] = dataPtrImg[2];

                dataPtrImg += nChan * Piece_positions[0] + widthStepImg * topRight[1];

                //Obter imagem por rodar.
                for (y = 0; y < heightPiece; y++)
                {
                    for (x = 0; x < widthPiece; x++)
                    {
                        dataPtrPiece[0] = dataPtrImg[0];
                        dataPtrPiece[1] = dataPtrImg[1];
                        dataPtrPiece[2] = dataPtrImg[2];

                        dataPtrPiece += nChan;
                        dataPtrImg += nChan;
                    }
                    dataPtrPiece += paddingPiece;
                    dataPtrImg += widthStepImg - (widthPiece * nChan);
                }
                //return piece;
                Image<Bgr, byte> pieceCopy = piece.Copy();
                Rotation(piece, pieceCopy, (float)angle, background);
                //return piece;

                // Bgr mBg = new Bgr(dataPtrImg[0], dataPtrImg[1], dataPtrImg[2]);
                // System.Drawing.PointF p = new System.Drawing.PointF(widthPiece / 2.0f, heightPiece / 2.0f);
                // pieceCopy = piece.Rotate(radiansToDegrees(angle), p, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, mBg, false);
                // piece = pieceCopy;
                // return piece;

                //int[] positionsOfPieceRotated = new int[4];
                int[] dummyPieces;
                int dummyNumberImages = 1;
                //dummyPieces.Add(positionsOfPieceRotated);


                int[,] matrix = imageFinder(piece, out dummyNumberImages, piecesLabels);
                //return piece;
                dummyPieces = getPositionsAfterRotation(matrix, heightPiece, widthPiece, piece, piecesLabels);
                
                Image<Bgr, byte> toReturn = getNormalPiece(piece, dummyPieces[0], dummyPieces[1], dummyPieces[2], dummyPieces[3]);
                //return toReturn;
                toReturn = getPieceAfterRotation(toReturn, correctWidth, correctHeight);
                return toReturn;
                // int resultWidth = toReturn.Width;
                // int resultHeigh = toReturn.Height;

                // if(resultWidth!=correctWidth || resultHeigh!=correctHeight) {
                //     if(resultWidth!=correctWidth && resultHeigh==correctHeight) {
                //         toReturn = cropWidth(toReturn,correctWidth);
                //     }else if(resultWidth!=correctWidth && resultHeigh==correctHeight) {
                //         toReturn = cropHeight(toReturn,correctHeight);
                //     } else {
                //         toReturn = cropWidthHeight(toReturn,correctWidth,correctHeight);
                //     }
                // }

                // dataPtrImg = (byte*)mImg.imageData.ToPointer();
                // int newWidth = (int)Math.Ceiling((Piece_positions[2] - bottomLeft[0])/Math.Cos(degreesToRadians(angle)));
                // int newHeight = bottomLeft[1] - Piece_positions[1];

                // Image<Bgr, byte> pieceTrimmed = new Image<Bgr, byte>(newWidth, newHeight);
                // MIplImage mTrimmed = pieceTrimmed.MIplImage;
                // byte* dataPtrTrimmed = (byte*)mTrimmed.imageData.ToPointer();
                // int widthStepTrimmed = mTrimmed.widthStep;
                // int paddingTrimmed = widthStepTrimmed - nChan * mTrimmed.width;

                // dataPtrPiece += nChan * falta_calcular + widthStepImg * (topRight[1]-Piece_positions[1]);
                // for(y=0; y<heightPiece; y++){
                //     for(x=0; x<widthPiece; x++){
                //         dataPtrTrimmed[0] = dataPtrPiece[0];
                //         dataPtrTrimmed[1] = dataPtrPiece[1];
                //         dataPtrTrimmed[2] = dataPtrPiece[2];

                //         dataPtrPiece += nChan;
                //         dataPtrTrimmed += nChan;
                //     }
                //     dataPtrPiece += widthStepImg - (widthPiece * nChan);
                //     dataPtrTrimmed += paddingTrimmed;
                // }

                //Falta recortar as bordas da imagem.

                //return piece;
            }
        }

        // private static Image<Bgr,byte> cropWidth(Image<Bgr,byte> piece, int correctWidth) {
        //     int width = piece.Width;
        //     int widthToCrop = width - correctWidth;
        // }

        // private static Image<Bgr,byte> cropHeight(Image<Bgr,byte> piece, int correctHeight) {

        // }

        // private static Image<Bgr,byte> cropWidthHeight(Image<Bgr,byte> piece, int correctWidth, int correctHeight) {

        // }

        /// </summary>
        /// Function that finds the rotation of each image, and returns the coordinates of the both upper corners.
        /// </summary>
        /// <param name="img">The puzzle eunsolved</param>
        /// <param name="Pieces_angle">The list to be filled with each piece's angle</param>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static List<double> findRotations(Image<Bgr, byte> img, int[,] matrix, int numberImages, List<int[]> upperRightCorners, List<int[]> upperLeftCorners)
        {
            List<double> Pieces_angle = new List<double>();

            //Gets the angle of rotation of each image
            double angle;
            int xTopLeft = 0;
            int yTopLeft = 0;
            int xTopRight = 0;
            int yTopRight = 0;

            for (int i = 0; i < numberImages; i++)
            {
                xTopLeft = upperLeftCorners[i][0];
                yTopLeft = upperLeftCorners[i][1];
                xTopRight = upperRightCorners[i][0];
                yTopRight = upperRightCorners[i][1];

                if (yTopLeft == yTopRight)
                {
                    Pieces_angle.Add(0);
                }
                else
                {
                    angle = Math.Atan((yTopLeft - yTopRight) * 1.0 / (xTopRight - xTopLeft));
                    Pieces_angle.Add(angle);
                }
            }
            return Pieces_angle;
        }

        private static double radiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        private static double degreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

/// </summary>
        /// Function that gives the coordinates of the bottom right corner, when the image is rotated and when is not.
        /// </summary>
        /// <param name="matrix">The matrix with labels</param>
        /// <param name="numberImages">The number of pieces in the image</param>
        private static List<int[]> getBottomRightCorners(Image<Bgr, byte> img, int[,] matrix, int numberImages, List<int> piecesLabels)
        {
            List<int[]> bottomCorners = new List<int[]>(numberImages);
            int[] a = new int[1];

            for (int i = 0; i < numberImages; i++)
            {
                bottomCorners.Add(a);
            }

            int height = img.Height;
            int width = img.Width;
            int x, y;
            int currentLabel = 0;
            List <int> seen = new List<int>();
            int numberSeen = 0;

            for (x = width - 1; x >= 0 && numberImages != numberSeen; x--)
            {
                for (y = height - 1; y >= 0 && numberImages != numberSeen; y--)
                {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && !seen.Contains(label))
                    {
                        bottomCorners[piecesLabels.IndexOf(label)] = new int[] { x, y };
                        currentLabel = label;
                        seen.Add(label);
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
        private static List<int[]> getBottomLeftCorners(Image<Bgr, byte> img, int[,] matrix, int numberImages, List<int> piecesLabels)
        {
            List<int[]> bottomCorners = new List<int[]>(numberImages);
            int[] a = new int[1];

            for (int i = 0; i < numberImages; i++)
            {
                bottomCorners.Add(a);
            }

            int height = img.Height;
            int width = img.Width;
            int x, y;
            int currentLabel = 0;
            List<int> seen = new List<int>();
            int numberSeen = 0;
            int[] coords = new int[2];

            for (y = height - 1; y >= 0 && numberImages != numberSeen; y--)
            {
                for (x = 0; x < width && numberImages != numberSeen; x++)
                {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && !seen.Contains(label))
                    {
                        coords[0] = x;
                        coords[1] = y;
                        bottomCorners[piecesLabels.IndexOf(label)] = coords;
                        currentLabel = label;
                        seen.Add(label);
                        numberSeen++;
                        coords = new int[2];
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
        private static List<int[]> getUpperLeftCorners(Image<Bgr, byte> img, int[,] matrix, int numberImages, List<int> piecesLabels)
        {
            List<int[]> upperCorners = new List<int[]>(numberImages);
            int[] a = new int[1];

            for (int i = 0; i < numberImages; i++)
            {
                upperCorners.Add(a);
            }

            int height = img.Height;
            int width = img.Width;
            int x, y;
            int currentLabel = 0;
            List<int> seen = new List<int>();
            int numberSeen = 0;
            int[] coords = new int[2];

            for (x = 0; x < width && numberImages != numberSeen; x++)
            {
                for (y = 0; y < height && numberImages != numberSeen; y++)
                {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && !seen.Contains(label))
                    {
                        coords[0] = x; //xStart
                        coords[1] = y; //yStart
                        upperCorners[piecesLabels.IndexOf(label)] = coords;
                        currentLabel = label;
                        seen.Add(label);
                        numberSeen++;
                        coords = new int[2];
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
        private static List<int[]> getUpperRightCorners(Image<Bgr, byte> img, int[,] matrix, int numberImages, List<int> piecesLabels)
        {
            List<int[]> upperCorners = new List<int[]>(numberImages);
            int[] a = new int[1];

            for (int i = 0; i < numberImages; i++)
            {
                upperCorners.Add(a);
            }

            int height = img.Height;
            int width = img.Width;
            int x, y;
            int currentLabel = 0;
            List<int> seen = new List<int>();
            int numberSeen = 0;
            int[] coords = new int[2];

            for (y = 0; y < height && numberImages != numberSeen; y++)
            {
                for (x = width - 1; x >= 0 && numberImages != numberSeen; x--)
                {
                    int label = matrix[x, y];
                    if (label != currentLabel && label != 0 && !seen.Contains(label))
                    {
                        coords[0] = x; //xStart
                        coords[1] = y; //yStart
                        upperCorners[piecesLabels.IndexOf(label)] = coords;
                        currentLabel = label;
                        seen.Add(label);
                        numberSeen++;
                        coords = new int[2];
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
        private static int leastDifference(int[] differences)
        {
            int smallestValue = differences[0];
            int smallestIndex = 0;

            for (int i = 1; i < differences.Length; i++)
            {
                if (differences[i] != -1 && differences[i] < smallestValue)
                {
                    smallestIndex = i;
                    smallestValue = differences[i];
                }
            }
            return smallestIndex;
        }

        private static Image<Bgr, byte> joinPiecesLevel2(List<Image<Bgr, byte>> pieces)
        {
            Image<Bgr, byte> resultImage = null;
            foreach (Image<Bgr, byte> piece in pieces)
            {

            }
            return resultImage;
        }

        private static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = value;
            }

            return arr;
        }
        private static Image<Bgr, byte> joinPiecesLevel3(List<Image<Bgr, byte>> pieces)
        {
            Image<Bgr, byte> resultImage = null;
            //{pieceIndex, top/right/bottom/left} = joinedPieceIndex
            int[][] solvedMatrix = new int[pieces.Count][];

            joinedPiecesMatrix(solvedMatrix, pieces);

            //Join pieces on the horizontal (left/right sides)
            List<Image<Bgr, byte>> horizontallyJoinedPieces = joinPiecesHorizontally(pieces, solvedMatrix);

            //Join pieces on the vertical (top/bottom sides)
            resultImage = joinPiecesVertically(horizontallyJoinedPieces);

            horizontallyJoinedPieces = null;

            return resultImage;
        }

        private static List<Image<Bgr, byte>> joinPiecesHorizontally(List<Image<Bgr, byte>> pieces, int[][] solvedMatrix)
        {
            List<Image<Bgr, byte>> joinedPieces = new List<Image<Bgr, byte>>();
            HashSet<int> joinedIndexes = new HashSet<int>();

            Image<Bgr, byte> resultImage = pieces[0];

            for (int i = 0; i < pieces.Count; i++)
            {
                if (!joinedIndexes.Contains(i))
                {
                    resultImage = pieces[i];
                    joinedIndexes.Add(i);
                    int right = solvedMatrix[i][1];
                    int left = solvedMatrix[i][3];

                    while (right != -1 || left != -1) //Nao sai daqui
                    {
                        if (right != -1)
                        { 
                          if (resultImage.Height != pieces[right].Height)
                            {
                                float scaleFactor = (float)resultImage.Height / pieces[right].Height;

                                Scale(pieces[right], pieces[right].Copy(), scaleFactor);
                            }
 
                            joinedIndexes.Add(right);
                            resultImage = joinLeftRight(resultImage, pieces[right]);
                            right = solvedMatrix[right][1];
                        }
                        if (left != -1)
                        { 
                            if (resultImage.Height != pieces[left].Height)
                            {
                                float scaleFactor = (float)resultImage.Height / pieces[left].Height;

                                Scale(pieces[left], pieces[left].Copy(), scaleFactor);
                            }

                            joinedIndexes.Add(left);
                            resultImage = joinLeftRight(resultImage, pieces[left]);
                            left = solvedMatrix[left][3];
                        }
                    }

                    joinedPieces.Add(resultImage);
                }
            }
            resultImage = null;
            return joinedPieces;
        }

        private static Image<Bgr, byte> joinPiecesVertically(List<Image<Bgr, byte>> pieces)
        {
            //{pieceIndex, left/right/top/bottom} = joinedPieceIndex
            int[][] solvedMatrix = new int[pieces.Count][];
            Image<Bgr, byte> resultImage = pieces[0];
            HashSet<int> joinedIndexes = new HashSet<int>();

            joinedPiecesMatrix(solvedMatrix, pieces);

            for (int i = 0; i < pieces.Count; ++i)
            {
                if (!joinedIndexes.Contains(i))
                {
                    resultImage = pieces[i];
                    joinedIndexes.Add(i);
                    int bottom = solvedMatrix[i][2];
                    int top = solvedMatrix[i][0];

                    while (bottom != -1 || top != -1)
                    {
                        if (bottom != -1)
                        {/* 
                            if (resultImage.Width != pieces[bottom].Width)
                            {
                                float scaleFactor = (float)resultImage.Width / pieces[bottom].Width;

                                Scale(pieces[bottom], pieces[bottom].Copy(), scaleFactor);
                            }*/

                            joinedIndexes.Add(bottom);
                            resultImage = joinTopBottom(resultImage, pieces[bottom]);
                            bottom = solvedMatrix[bottom][2];
                        }
                        if (top != -1)
                        {/* 
                            
                            if (resultImage.Width != pieces[top].Width)
                            {
                                float scaleFactor = (float)resultImage.Width / pieces[top].Width;

                                Scale(pieces[top], pieces[top].Copy(), scaleFactor);
                            }*/

                            joinedIndexes.Add(top);
                            resultImage = joinTopBottom(resultImage, pieces[top]);
                            top = solvedMatrix[top][0];
                        }
                    }
                }
            }

            return resultImage;
        }

        private static void joinedPiecesMatrix(int[][] matrix, List<Image<Bgr, byte>> pieces)
        {
            //{piece1Index, piece2Index, top/right/bottom/left,} = border difference
            int[][] diffMatrix = new int[pieces.Count][];

            //Populate the matrix so that each piece isn't joined to any piece (-1)
            for (int i = 0; i < matrix.Length; ++i)
            {
                matrix[i] = new int[4];
                for(int j=0; j < 4; ++j)
                {
                    matrix[i][j] = -1;
                }
                
            }

            for (int i = 0; i < diffMatrix.Length; ++i)
            {
                diffMatrix[i] = new int[4];
                for (int j = 0; j < 4; ++j)
                {
                    diffMatrix[i][j] = -1;
                }

            }
            

            for (int i = 0; i < pieces.Count - 1; ++i)
            {
                for (int j = i + 1; j < pieces.Count; ++j)
                {
                    int[] differences = new int[4];
                    int leastDiff = leastDifferenceLevel3(pieces[i], pieces[j], differences);

                    if (leastDiff != -1 && (matrix[i][leastDiff] == -1 || (differences[leastDiff] != -1 && differences[leastDiff] < diffMatrix[i][leastDiff])))
                    {
                        unsafe
                        {

                            int a = 0;
                            int* temp = &a;
                            *temp = matrix[i][leastDiff];

                            //int temp = matrix[i][leastDiff];
                            matrix[i][leastDiff] = new int();
                            // *temp = differences[leastDiff];
                            matrix[i][leastDiff] = j;
                            matrix[j][(leastDiff + 2) % 4] = i;

                            if (*temp != -1)
                            {
                                matrix[*temp][(leastDiff + 2) % 4] = -1;
                                diffMatrix[*temp][(leastDiff + 2) % 4] = -1;

                            }
                        



                        diffMatrix[i][leastDiff] = differences[leastDiff];
                        diffMatrix[j][(leastDiff + 2) % 4] = differences[leastDiff];
                        }
                    }
                }
            }
        }

        private static int leastDifferenceLevel3(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2, int[] differences)
        {
            int[] differencesLeftRight, differencesTopBottom;
            
            if (piece1.Width == piece2.Width && piece1.Height == piece2.Height)
            {
                differencesTopBottom = checkTopBottom(piece1, piece2);
                differencesLeftRight = checkLeftRight(piece1, piece2);

                differences[0] = differencesTopBottom[0];
                differences[1] = differencesLeftRight[1];
                differences[2] = differencesTopBottom[1];
                differences[3] = differencesLeftRight[0];
                return leastDifference(differences);
            } else if (piece1.Width == piece2.Width)
            {
                differencesTopBottom = checkTopBottom(piece1, piece2);
                differences[0] = differencesTopBottom[0];
                differences[1] = -1;
                differences[2] = differencesTopBottom[1];
                differences[3] = -1;
                
                int diff = leastDifference(differencesTopBottom);
                if(diff == 0) {
                    return 0; //top of piece1
                } else {
                    return 2; //bottom of piece1
                }                                                                                           
                
            } else if (piece1.Height == piece2.Height)
            {
                differencesLeftRight = checkLeftRight(piece1, piece2);
                differences[0] = -1;
                differences[1] = differencesLeftRight[1];
                differences[2] = -1;
                differences[3] = differencesLeftRight[0];
               
                int diff = leastDifference(differencesLeftRight);

                if(diff == 0) {
                    return 3; //left of piece1
                } else {
                    return 1; //right of piece1
                }
            }
            
            return -1;
        }

        /// </summary>
        /// Function that resolves a level 1 puzzle, givem the pieces of the puzzle
        /// </summary>
        /// <param name="piece1">One piece to be joint</param>
        /// <param name="piece2">Another piece to be joint</param>
        public static Image<Bgr, byte> joinPiecesLevel1(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2)
        {
            Image<Bgr, byte> dummyImg = null;
            MIplImage mPiece1 = piece1.MIplImage;
            MIplImage mPiece2 = piece2.MIplImage;
            int widthPiece1 = mPiece1.width;
            int heightPiece1 = mPiece1.height;
            int widthPiece2 = mPiece2.width;
            int heightPiece2 = mPiece2.height;
            int[] differencesLeftRight = null;
            int[] differencesTopBottom = null;

            //If the two pieces have the exact same measures
            if (widthPiece1 == widthPiece2 && heightPiece1 == heightPiece2)
            {
                //Index0 is the difference between left of image1 and right of image2
                //Index1 is the difference between right of image1 and left of image2
                //Index2 is the difference between top of image1 and bottom of image2
                //Index3 is the difference between bottom of image1 and top of image2                
                int[] differences = new int[4];
                differencesLeftRight = checkLeftRight(piece1, piece2);
                differencesTopBottom = checkTopBottom(piece1, piece2);
                differences[0] = differencesLeftRight[0];
                differences[1] = differencesLeftRight[1];
                differences[2] = differencesTopBottom[0];
                differences[3] = differencesTopBottom[1];

                int leastDiff = leastDifference(differences);

                //if the joint is to be made between right and left sides
                if (leastDiff < 2)
                {

                    if (leastDiff == 0)
                    {
                        dummyImg = joinLeftRight(piece2, piece1);
                    }
                    else
                    {
                        dummyImg = joinLeftRight(piece1, piece2);
                    }

                    //If the joint is to be made between top and bottom sides
                }
                else
                {

                    if (leastDiff == 2)
                    {
                        dummyImg = joinTopBottom(piece2, piece1);
                    }
                    else
                    {
                        dummyImg = joinTopBottom(piece1, piece2);
                    }
                }

                //If the two pieces only have the same width                                         
            }
            else if (widthPiece1 == widthPiece2)
            {
                //Index0 is the difference between top of image1 and bottom of image2
                //Index1 is the difference between bottom of image1 and top of image2                            
                differencesTopBottom = checkTopBottom(piece1, piece2);

                if (differencesTopBottom[0] < differencesTopBottom[1])
                {
                    dummyImg = joinTopBottom(piece2, piece1);
                }
                else
                {
                    dummyImg = joinTopBottom(piece1, piece2);
                }

                //If the two pieces only have the samee height
            }
            else
            {
                //Index0 is the difference between left of image1 and right of image2
                //Index1 is the difference between right of image1 and left of image2                
                differencesLeftRight = checkLeftRight(piece1, piece2);

                if (differencesLeftRight[0] > differencesLeftRight[1])
                {
                    dummyImg = joinLeftRight(piece1, piece2);
                }
                else
                {
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
        private static Image<Bgr, byte> joinTopBottom(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2)
        {
            unsafe
            {
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
                for (y = 0; y < heightPiece1; y++)
                {
                    for (x = 0; x < totalWidth; x++)
                    {
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
                for (y = heightPiece1; y < totalHeight; y++)
                {
                    for (x = 0; x < totalWidth; x++)
                    {
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
        private static Image<Bgr, byte> joinLeftRight(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2)
        {
            unsafe
            {
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

                for (y = 0; y < totalHeight; y++)
                {
                    //Copies to dummyImg the left piece
                    for (x = 0; x < widthPiece1; x++)
                    {
                        dataPtrDummy[0] = dataPtr1[0];
                        dataPtrDummy[1] = dataPtr1[1];
                        dataPtrDummy[2] = dataPtr1[2];
                        dataPtr1 += nChan;
                        dataPtrDummy += nChan;
                    }

                    //Copies to dummyImg the right piece
                    for (x = widthPiece1; x < totalWidth; x++)
                    {
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

        public static int[] checkLeftRight(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2)
        {
            unsafe
            {
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
                dataPtr2 += nChan * (widthPiece2-1); //Goes to right side of piece2
                for (int y = 0; y < height; y++)
                {
                    differencesLeftRight[0] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += widthStepPiece1;
                    dataPtr2 += widthStepPiece2;
                }

                dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                //Compare right of image1 to left of image2
                dataPtr1 += nChan * (widthPiece1-1);
                for (int y = 0; y < height; y++)
                {
                    differencesLeftRight[1] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += widthStepPiece1;
                    dataPtr2 += widthStepPiece2;
                }
                return differencesLeftRight;
            }
        }

        public static int[] checkTopBottom(Image<Bgr, byte> piece1, Image<Bgr, byte> piece2)
        {
            unsafe
            {
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
                dataPtr2 += widthStepPiece2 * (heightPiece2-1);
                for (int x = 0; x < width; x++)
                {
                    differencesTopBottom[0] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += nChan;
                    dataPtr2 += nChan;
                }

                dataPtr1 = (byte*)mPiece1.imageData.ToPointer();
                dataPtr2 = (byte*)mPiece2.imageData.ToPointer();

                //Compare bottom of image1 to top of image2
                dataPtr1 += widthStepPiece1 * (heightPiece1-1);
                for (int x = 0; x < width; x++)
                {
                    differencesTopBottom[1] += Math.Abs(dataPtr2[0] - dataPtr1[0]) + Math.Abs(dataPtr2[1] - dataPtr1[1]) + Math.Abs(dataPtr2[2] - dataPtr1[2]);
                    dataPtr1 += nChan;
                    dataPtr2 += nChan;
                }

                return differencesTopBottom;
            }
        }
        private static int[,] initiateLabelMatrix(MIplImage mStart, int[] bgrBackground, out int changes, out int numberImages)
        {
            int nChan = mStart.nChannels;
            int padding = mStart.widthStep - mStart.nChannels * mStart.width;
            int widthStep = mStart.widthStep;
            int width = mStart.width;
            int height = mStart.height;
            int x, y, left, topRight, top, topLeft, label;
            int[,] matrix = new int[width, height];
            changes = 0;
            numberImages = 0;
            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();

                //To ignore borders
                dataPtr += nChan + widthStep;


                for (y = 1; y < height - 1; y++)
                {
                    for (x = 1; x < width - 1; x++)
                    {
                        if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                        {
                            changes = 1;
                            left = matrix[x - 1, y];
                            top = matrix[x, y - 1];
                            topLeft = matrix[x - 1, y - 1];
                            topRight = matrix[x + 1, y - 1];
                            label = width * height;

                            if (top != 0 && top < label)
                            {
                                label = top;
                            }
                            if (left != 0 && left < label)
                            {
                                label = left;
                            }
                            if (topLeft != 0 && topLeft < label)
                            {
                                label = topLeft;
                            }
                            if (topRight != 0 && topRight < label)
                            {
                                label = topRight;
                            }

                            if (label == width * height && matrix[x, y] == 0)
                            {
                                label = ++numberImages;
                            }

                            matrix[x, y] = label;

                        }
                        dataPtr += nChan;
                    }
                    dataPtr += padding + nChan * 2;
                }

                return matrix;
            }
        }
        public static int[,] imageFinder(Image<Bgr, byte> img, out int numberImages, List<int> piecesLabels)
        {
            unsafe
            {
                MIplImage mStart = img.MIplImage;
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                int changes;
                int[] bgrBackground = { dataPtr[0], dataPtr[1], dataPtr[2] };

                //Algoritmo dos componentes ligados iterativo
                int[,] matrix = initiateLabelMatrix(mStart, bgrBackground, out changes, out numberImages);
                propragateLabels(mStart, matrix, changes);

                labelBorders(mStart, matrix, bgrBackground);

                numberImages = getNumberOfImages(mStart, matrix, piecesLabels);

                return matrix;
            }
        }

        private static void labelBorders(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            labelUpperBorder(mStart, matrix, bgrBackground);
            labelTopRightCorner(mStart, matrix, bgrBackground);
            labelRightBorder(mStart, matrix, bgrBackground);
            labelBottomRightCorner(mStart, matrix, bgrBackground);
            labelBottomBorder(mStart, matrix, bgrBackground);
            labelBottomLeftCorner(mStart, matrix, bgrBackground);
            labelLeftBorder(mStart, matrix, bgrBackground);
        }
        private static void propragateLabels(MIplImage mStart, int[,] matrix, int changes)
        {
            unsafe
            {
                while (changes == 1)
                {
                    propagateTopDown(mStart, matrix, out changes);

                    if (changes == 1)
                    {
                        propagateBottomUp(mStart, matrix, out changes);
                    }
                }
            }
        }
        private static void propagateTopDown(MIplImage mStart, int[,] matrix, out int changes)
        {
            int left, bottom, right, top, label, topLeft, topRight, botLeft, botRight, x, y;

            unsafe
            {
                changes = 0;

                for (y = 1; y < mStart.height - 1; y++)
                {
                    for (x = 1; x < mStart.width - 1; x++)
                    {

                        if (matrix[x, y] != 0)
                        {
                            right = matrix[x + 1, y];
                            bottom = matrix[x, y + 1];
                            botLeft = matrix[x - 1, y + 1];
                            botRight = matrix[x + 1, y + 1];
                            left = matrix[x - 1, y];
                            top = matrix[x, y - 1];
                            topLeft = matrix[x - 1, y - 1];
                            topRight = matrix[x + 1, y - 1];
                            label = matrix[x, y];

                            if (top != 0 && top < label)
                            {
                                label = top;
                            }
                            if (left != 0 && left < label)
                            {
                                label = left;
                            }
                            if (topLeft != 0 && topLeft < label)
                            {
                                label = topLeft;
                            }
                            if (topRight != 0 && topRight < label)
                            {
                                label = topRight;
                            }
                            if (bottom != 0 && bottom < label)
                            {
                                label = bottom;
                            }
                            if (right != 0 && right < label)
                            {
                                label = right;
                            }
                            if (botLeft != 0 && botLeft < label)
                            {
                                label = botLeft;
                            }
                            if (botRight != 0 && botRight < label)
                            {
                                label = botRight;
                            }
                            if (matrix[x, y] != label) //If matrix was changed
                            {
                                changes = 1;
                                matrix[x, y] = label;
                            }
                        }
                    }
                }
            }
        }

        private static void propagateBottomUp(MIplImage mStart, int[,] matrix, out int changes)
        {
            int left, bottom, right, top, label, topLeft, topRight, botLeft, botRight, x, y;

            unsafe
            {
                changes = 0;

                for (y = mStart.height - 2; y > 0; y--)
                {
                    for (x = mStart.width - 2; x > 0; x--)
                    {
                        if (matrix[x, y] != 0)
                        {
                            right = matrix[x + 1, y];
                            bottom = matrix[x, y + 1];
                            botLeft = matrix[x - 1, y + 1];
                            botRight = matrix[x + 1, y + 1];
                            left = matrix[x - 1, y];
                            top = matrix[x, y - 1];
                            topLeft = matrix[x - 1, y - 1];
                            topRight = matrix[x + 1, y - 1];
                            label = matrix[x, y];

                            if (top != 0 && top < label)
                            {
                                label = top;
                            }
                            if (left != 0 && left < label)
                            {
                                label = left;
                            }
                            if (topLeft != 0 && topLeft < label)
                            {
                                label = topLeft;
                            }
                            if (topRight != 0 && topRight < label)
                            {
                                label = topRight;
                            }
                            if (bottom != 0 && bottom < label)
                            {
                                label = bottom;
                            }
                            if (right != 0 && right < label)
                            {
                                label = right;
                            }
                            if (botLeft != 0 && botLeft < label)
                            {
                                label = botLeft;
                            }
                            if (botRight != 0 && botRight < label)
                            {
                                label = botRight;
                            }
                            if (matrix[x, y] != label) //If matrix was changed
                            {
                                changes = 1;
                                matrix[x, y] = label;
                            }
                        }
                    }
                }
            }
        }
        private static int getNumberOfImages(MIplImage mStart, int[,] matrix, List<int> piecesLabels)
        {
            int numberImages = 0;
            int width = mStart.width;
            int height = mStart.height;
            HashSet<int> labelsSeen = new HashSet<int>();
            int label, x, y;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();

                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        label = matrix[x, y];
                        if (label != 0 && !labelsSeen.Contains(label))
                        {
                            labelsSeen.Add(label);
                            piecesLabels.Add(label);
                            numberImages++;
                        }
                    }
                }

                return numberImages;
            }
        }
        private static void labelBottomRightCorner(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int left, top, label, x, y;
            int widthStep = mStart.widthStep;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height - 1) + nChan * (width - 1);
                x = width - 1;
                y = height - 1;
                if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                {
                    top = matrix[x, y - 1];
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

                    matrix[x, y] = label;
                }
            }
        }
        private static void labelBottomLeftCorner(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int right, top, label, y;
            int widthStep = mStart.widthStep;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep * (height - 1);
                y = height - 1;
                if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                {
                    top = matrix[0, y - 1];
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

                    matrix[0, y] = label;
                }
            }
        }
        private static void labelTopRightCorner(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int left, bottom, label, x;
            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan * (width - 1);
                x = width - 1;
                if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                {
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
            }
        }

        private static void labelUpperBorder(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int left, bottom, label;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan;

                for (int x = 1; x < width - 1; x++)
                {

                    if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                    {
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
            }
        }
        private static void labelRightBorder(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int widthStep = mStart.widthStep;
            int left, top, label, x;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep;
                x = width - 1;
                for (int y = 1; y < height - 1; y++)
                {

                    if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                    {
                        left = matrix[x - 1, y];
                        top = matrix[x, y - 1];
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
                    dataPtr += widthStep;
                }
            }
        }
        private static void labelBottomBorder(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int left, top, label;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += nChan;

                for (int x = 1; x < width - 1; x++)
                {

                    if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                    {
                        left = matrix[x - 1, height - 1];
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
                    dataPtr += nChan;
                }
            }
        }
        private static void labelLeftBorder(MIplImage mStart, int[,] matrix, int[] bgrBackground)
        {
            int nChan = mStart.nChannels;
            int width = mStart.width;
            int height = mStart.height;
            int widthStep = mStart.widthStep;
            int right, top, label, x;

            unsafe
            {
                byte* dataPtr = (byte*)mStart.imageData.ToPointer();
                dataPtr += widthStep;
                x = 0;
                for (int y = 1; y < height - 1; y++)
                {

                    if (dataPtr[0] != bgrBackground[0] || dataPtr[1] != bgrBackground[1] || dataPtr[2] != bgrBackground[2])
                    {
                        top = matrix[x, y - 1];
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

                        matrix[x, y] = label;
                    }
                    dataPtr += widthStep;
                }
            }
        }

        private static int[] getPositionsAfterRotation(int[,] matrix, int height, int width,Image<Bgr, byte> img, List<int> piecesLabels) {
            int[] upperLeftCorner = getUpperLeftCorners(img, matrix, 1, piecesLabels)[0];
            int[] upperRightCorners = getUpperRightCorners(img, matrix, 1,piecesLabels)[0];
            int[] lowerRightCorner = getBottomRightCorners(img, matrix, 1,piecesLabels)[0];
            int[] lowerLeftCorner = getBottomLeftCorners(img,matrix,1,piecesLabels)[0];
            int[] currPiecePositions = new int[4];

            currPiecePositions[0] = Math.Min(upperLeftCorner[0],upperRightCorners[0]);
            currPiecePositions[1] = Math.Min(upperLeftCorner[1],upperRightCorners[1]);
            currPiecePositions[2] = Math.Max(lowerRightCorner[0],lowerLeftCorner[0]);
            currPiecePositions[3] = Math.Max(lowerRightCorner[1],lowerLeftCorner[1]);

            return currPiecePositions;
        }
        private static List<int[]> getPositions(int[,] matrix, int height, int width, int numberImages, Image<Bgr, byte> img, List<int> piecesLabels)
        {
            List<int[]> Pieces_positions = new List<int[]>();
            List<int[]> upperLeftCorner = getUpperLeftCorners(img, matrix, numberImages,piecesLabels);
            List<int[]> lowerRightCorner = getBottomRightCorners(img, matrix, numberImages,piecesLabels);
            int[] currPiecePositions = new int[4];

            for (int i = 0; i < numberImages; i++)
            {
                currPiecePositions[0] = upperLeftCorner[i][0];
                currPiecePositions[1] = upperLeftCorner[i][1];
                currPiecePositions[2] = lowerRightCorner[i][0];
                currPiecePositions[3] = lowerRightCorner[i][1];
                Pieces_positions.Add(currPiecePositions);
                currPiecePositions = new int[4];
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

        public static void Rotation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float angle, byte[] rgb)
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
                                dataPtr[0] = rgb[0];
                                dataPtr[1] = rgb[1];
                                dataPtr[2] = rgb[2];
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
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage mCopy = imgCopy.MIplImage;

                int width = imgCopy.Width;
                int height = imgCopy.Height;
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                byte* dataPtrCopy = (byte*)mCopy.imageData.ToPointer();
                int nChannels = m.nChannels;

                if (nChannels == 3)
                {
                    int[] tempArray = new int[3];
                    dataPtr += nChannels + m.widthStep;
                    dataPtrCopy += nChannels + m.widthStep;

                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            tempArray[0] = Math.Abs(
                                ((dataPtrCopy - m.widthStep - nChannels)[0] + 2 * (dataPtrCopy - nChannels)[0] + (dataPtrCopy + m.widthStep - nChannels)[0]) -
                                ((dataPtrCopy - m.widthStep + nChannels)[0] + 2 * (dataPtrCopy + nChannels)[0] + (dataPtrCopy + m.widthStep + nChannels)[0])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[0] + 2 * (dataPtrCopy + m.widthStep)[0] + (dataPtrCopy + m.widthStep + nChannels)[0]) -
                                ((dataPtrCopy - m.widthStep - nChannels)[0] + 2 * (dataPtrCopy - m.widthStep)[0] + (dataPtrCopy - m.widthStep + nChannels)[0]));

                            tempArray[1] = Math.Abs(
                                ((dataPtrCopy - m.widthStep - nChannels)[1] + 2 * (dataPtrCopy - nChannels)[1] + (dataPtrCopy + m.widthStep - nChannels)[1]) -
                                ((dataPtrCopy - m.widthStep + nChannels)[1] + 2 * (dataPtrCopy + nChannels)[1] + (dataPtrCopy + m.widthStep + nChannels)[1])) +

                               Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[1] + 2 * (dataPtrCopy + m.widthStep)[1] + (dataPtrCopy + m.widthStep + nChannels)[1]) -
                                ((dataPtrCopy - m.widthStep - nChannels)[1] + 2 * (dataPtrCopy - m.widthStep)[1] + (dataPtrCopy - m.widthStep + nChannels)[1]));

                            tempArray[2] = Math.Abs(
                                ((dataPtrCopy - m.widthStep - nChannels)[2] + 2 * (dataPtrCopy - nChannels)[2] + (dataPtrCopy + m.widthStep - nChannels)[2]) -
                                ((dataPtrCopy - m.widthStep + nChannels)[2] + 2 * (dataPtrCopy + nChannels)[2] + (dataPtrCopy + m.widthStep + nChannels)[2])) +

                               Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[2] + 2 * (dataPtrCopy + m.widthStep)[2] + (dataPtrCopy + m.widthStep + nChannels)[2]) -
                                ((dataPtrCopy - m.widthStep - nChannels)[2] + 2 * (dataPtrCopy - m.widthStep)[2] + (dataPtrCopy - m.widthStep + nChannels)[2]));

                            if (tempArray[0] > 255)
                            {
                                dataPtr[0] = 255;
                            }
                            else if (tempArray[0] < 0)
                            {
                                dataPtr[0] = 0;
                            }
                            else
                            {
                                dataPtr[0] = (byte)tempArray[0];
                            }

                            if (tempArray[1] > 255)
                            {
                                dataPtr[1] = 255;
                            }
                            else if (tempArray[1] < 0)
                            {
                                dataPtr[1] = 0;
                            }
                            else
                            {
                                dataPtr[1] = (byte)tempArray[1];
                            }

                            if (tempArray[2] > 255)
                            {
                                dataPtr[2] = 255;
                            }
                            else if (tempArray[2] < 0)
                            {
                                dataPtr[2] = 0;
                            }
                            else
                            {
                                dataPtr[2] = (byte)tempArray[2];
                            }

                            dataPtr += nChannels;
                            dataPtrCopy += nChannels;
                        }

                        dataPtr += 2 * nChannels + padding;
                        dataPtrCopy += 2 * nChannels + padding;
                    }

                    // //Top Left Corner
                    // dataPtr = (byte*)m.imageData.ToPointer();
                    // dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    // tempArray[0] = Math.Abs(
                    //         ((dataPtrCopy)[0] + 2 * (dataPtrCopy)[0] + (dataPtrCopy + m.widthStep)[0]) -
                    //         ((dataPtrCopy + nChannels)[0] + 2 * (dataPtrCopy + nChannels)[0] + (dataPtrCopy + m.widthStep + nChannels)[0])) +

                    //        Math.Abs(
                    //         ((dataPtrCopy + m.widthStep)[0] + 2 * (dataPtrCopy + m.widthStep)[0] + (dataPtrCopy + m.widthStep + nChannels)[0]) -
                    //         ((dataPtrCopy)[0] + 2 * (dataPtrCopy)[0] + (dataPtrCopy + nChannels)[0]));

                    // tempArray[1] = Math.Abs(
                    //         ((dataPtrCopy)[1] + 2 * (dataPtrCopy2)[1] + (dataPtrCopy2 + m.widthStep)[1]) -
                    //         ((dataPtrCopy + nChannels)[1] + 2 * (dataPtrCopy2 + nChannels)[1] + (dataPtrCopy + m.widthStep + nChannels)[1]) +

                    //        Math.Abs(
                    //         ((dataPtrCopy + m.widthStep)[1] + 2 * (dataPtrCopy + m.widthStep)[1] + (dataPtrCopy + m.widthStep + nChannels)[1]) -
                    //         ((dataPtrCopy)[1] + 2 * (dataPtrCopy)[1] + (dataPtrCopy + nChannels)[1])));

                    // tempArray[2] = Math.Abs(
                    //             ((dataPtrCopy)[2] + 2 * (dataPtrCopy)[2] + (dataPtrCopy + m.widthStep)[2]) -
                    //             ((dataPtrCopy + nChannels)[2] + 2 * (dataPtrCopy + nChannels)[2] + (dataPtrCopy + m.widthStep + nChannels)[2]) +

                    //            Math.Abs(
                    //             ((dataPtrCopy + m.widthStep)[2] + 2 * (dataPtrCopy + m.widthStep)[2] + (dataPtrCopy + m.widthStep + nChannels)[2]) -
                    //             ((dataPtrCopy)[2] + 2 * (dataPtrCopy)[2] + (dataPtrCopy + nChannels)[2])));


                    // if (tempArray[0] > 255)
                    // {
                    //     dataPtr2[0] = 255;
                    // }
                    // else if (tempArray[0] < 0)
                    // {
                    //     dataPtr2[0] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[0] = (byte)tempArray[0];
                    // }

                    // if (tempArray[1] > 255)
                    // {
                    //     dataPtr2[1] = 255;
                    // }
                    // else if (tempArray[1] < 0)
                    // {
                    //     dataPtr2[1] = 0; 
                    // }
                    // else
                    // {
                    //     dataPtr2[1] = (byte)tempArray[1];
                    // }

                    // if (tempArray[2] > 255)
                    // {
                    //     dataPtr2[2] = 255;
                    // }
                    // else if (tempArray[2] < 0)
                    // {
                    //     dataPtr2[2] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[2] = (byte)tempArray[2];
                    // }

                    // //Top Right Corner
                    // dataPtr2 = (dataPtr + height * m.widthStep + 0 * nChannels);
                    // dataPtrCopy2 = (dataPtrCopy + height * m.widthStep + 0 * nChannels);

                    // tempArray[0] = Math.Abs(
                    //         ((dataPtrCopy2 - m.widthStep - nChannels)[0] +
                    //         2 * (dataPtrCopy2 - nChannels)[0] +
                    //         (dataPtrCopy2 + m.widthStep - nChannels)[0]) -

                    //         ((dataPtrCopy2)[0] +
                    //         2 * (dataPtrCopy2)[0] +
                    //         (dataPtrCopy2 + m.widthStep)[0]) +

                    //        Math.Abs(
                    //         ((dataPtrCopy2 + m.widthStep - nChannels)[0] +
                    //         2 * (dataPtrCopy2 + m.widthStep)[0] +
                    //         (dataPtrCopy2 + m.widthStep)[0]) -

                    //         ((dataPtrCopy2 - nChannels)[0] +
                    //         2 * (dataPtrCopy2)[0] +
                    //         (dataPtrCopy2)[0])));

                    // tempArray[1] = Math.Abs(
                    //        ((dataPtrCopy2 - m.widthStep - nChannels)[1] +
                    //        2 * (dataPtrCopy2 - nChannels)[1] +
                    //        (dataPtrCopy2 + m.widthStep - nChannels)[1]) -

                    //        ((dataPtrCopy2)[1] +
                    //        2 * (dataPtrCopy2)[1] +
                    //        (dataPtrCopy2 + m.widthStep)[1]) +

                    //       Math.Abs(
                    //        ((dataPtrCopy2 + m.widthStep - nChannels)[1] +
                    //        2 * (dataPtrCopy2 + m.widthStep)[1] +
                    //        (dataPtrCopy2 + m.widthStep)[1]) -

                    //        ((dataPtrCopy2 - nChannels)[1] +
                    //        2 * (dataPtrCopy2)[1] +
                    //        (dataPtrCopy2)[1])));

                    // tempArray[2] = Math.Abs(
                    //        ((dataPtrCopy2 - m.widthStep - nChannels)[2] +
                    //        2 * (dataPtrCopy2 - nChannels)[2] +
                    //        (dataPtrCopy2 + m.widthStep - nChannels)[2]) -

                    //        ((dataPtrCopy2)[2] +
                    //        2 * (dataPtrCopy2)[2] +
                    //        (dataPtrCopy2 + m.widthStep)[2]) +

                    //       Math.Abs(
                    //        ((dataPtrCopy2 + m.widthStep - nChannels)[2] +
                    //        2 * (dataPtrCopy2 + m.widthStep)[2] +
                    //        (dataPtrCopy2 + m.widthStep)[2]) -

                    //        ((dataPtrCopy2 - nChannels)[2] +
                    //        2 * (dataPtrCopy2)[2] +
                    //        (dataPtrCopy2)[2])));


                    // if (tempArray[0] > 255)
                    // {
                    //     dataPtr2[0] = 255;
                    // }
                    // else if (tempArray[0] < 0)
                    // {
                    //     dataPtr2[0] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[0] = (byte)tempArray[0];
                    // }

                    // if (tempArray[1] > 255)
                    // {
                    //     dataPtr2[1] = 255;
                    // }
                    // else if (tempArray[1] < 0)
                    // {
                    //     dataPtr2[1] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[1] = (byte)tempArray[1];
                    // }

                    // if (tempArray[2] > 255)
                    // {
                    //     dataPtr2[2] = 255;
                    // }
                    // else if (tempArray[2] < 0)
                    // {
                    //     dataPtr2[2] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[2] = (byte)tempArray[2];
                    // }

                    // //Bottom Left Corner

                    // //Bottom Right Corner
                    // dataPtr2 = (dataPtr + height * m.widthStep + 0 * nChannels);
                    // dataPtrCopy2 = (dataPtrCopy + height * m.widthStep + 0 * nChannels);

                    // tempArray[0] = Math.Abs(
                    //         ((dataPtrCopy2 - m.widthStep)[0] +
                    //         2 * (dataPtrCopy2)[0] +
                    //         (dataPtrCopy2)[0]) -

                    //         ((dataPtrCopy2 - m.widthStep + nChannels)[0] +
                    //         2 * (dataPtrCopy2 + nChannels)[0] +
                    //         (dataPtrCopy2 + m.widthStep + nChannels)[0]) +

                    //        Math.Abs(
                    //         ((dataPtrCopy2 + m.widthStep - nChannels)[0] +
                    //         2 * (dataPtrCopy2 + m.widthStep)[0] +
                    //         (dataPtrCopy2 + m.widthStep)[0]) -

                    //         ((dataPtrCopy2 - nChannels)[0] +
                    //         2 * (dataPtrCopy2)[0] +
                    //         (dataPtrCopy2)[0])));

                    // tempArray[1] = Math.Abs(
                    //        ((dataPtrCopy2 - m.widthStep - nChannels)[1] +
                    //        2 * (dataPtrCopy2 - nChannels)[1] +
                    //        (dataPtrCopy2 + m.widthStep - nChannels)[1]) -

                    //        ((dataPtrCopy2)[1] +
                    //        2 * (dataPtrCopy2)[1] +
                    //        (dataPtrCopy2 + m.widthStep)[1]) +

                    //       Math.Abs(
                    //        ((dataPtrCopy2 + m.widthStep - nChannels)[1] +
                    //        2 * (dataPtrCopy2 + m.widthStep)[1] +
                    //        (dataPtrCopy2 + m.widthStep)[1]) -

                    //        ((dataPtrCopy2 - nChannels)[1] +
                    //        2 * (dataPtrCopy2)[1] +
                    //        (dataPtrCopy2)[1])));

                    // tempArray[2] = Math.Abs(
                    //        ((dataPtrCopy2 - m.widthStep - nChannels)[2] +
                    //        2 * (dataPtrCopy2 - nChannels)[2] +
                    //        (dataPtrCopy2 + m.widthStep - nChannels)[2]) -

                    //        ((dataPtrCopy2)[2] +
                    //        2 * (dataPtrCopy2)[2] +
                    //        (dataPtrCopy2 + m.widthStep)[2]) +

                    //       Math.Abs(
                    //        ((dataPtrCopy2 + m.widthStep - nChannels)[2] +
                    //        2 * (dataPtrCopy2 + m.widthStep)[2] +
                    //        (dataPtrCopy2 + m.widthStep)[2]) -

                    //        ((dataPtrCopy2 - nChannels)[2] +
                    //        2 * (dataPtrCopy2)[2] +
                    //        (dataPtrCopy2)[2])));


                    // if (tempArray[0] > 255)
                    // {
                    //     dataPtr2[0] = 255;
                    // }
                    // else if (tempArray[0] < 0)
                    // {
                    //     dataPtr2[0] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[0] = (byte)tempArray[0];
                    // }

                    // if (tempArray[1] > 255)
                    // {
                    //     dataPtr2[1] = 255;
                    // }
                    // else if (tempArray[1] < 0)
                    // {
                    //     dataPtr2[1] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[1] = (byte)tempArray[1];
                    // }

                    // if (tempArray[2] > 255)
                    // {
                    //     dataPtr2[2] = 255;
                    // }
                    // else if (tempArray[2] < 0)
                    // {
                    //     dataPtr2[2] = 0;
                    // }
                    // else
                    // {
                    //     dataPtr2[2] = (byte)tempArray[2];
                    // }

                    //Top Line

                    dataPtr = (byte*)m.imageData.ToPointer();
                    dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    dataPtr += nChannels;
                    dataPtrCopy += nChannels;

                    for (int x = 1; x < width - 1; x++)
                    {
                        tempArray[0] = Math.Abs(
                                ((dataPtrCopy - nChannels)[0] + 2 * (dataPtrCopy - nChannels)[0] + (dataPtrCopy + m.widthStep - nChannels)[0]) -
                                ((dataPtrCopy + nChannels)[0] + 2 * (dataPtrCopy + nChannels)[0] + (dataPtrCopy + m.widthStep + nChannels)[0])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[0] + 2 * (dataPtrCopy + m.widthStep)[0] + (dataPtrCopy + m.widthStep + nChannels)[0]) -
                                ((dataPtrCopy - nChannels)[0] + 2 * (dataPtrCopy)[0] + (dataPtrCopy + nChannels)[0]));

                        tempArray[1] = Math.Abs(
                                ((dataPtrCopy - nChannels)[1] + 2 * (dataPtrCopy - nChannels)[1] + (dataPtrCopy + m.widthStep - nChannels)[1]) -
                                ((dataPtrCopy + nChannels)[1] + 2 * (dataPtrCopy + nChannels)[1] + (dataPtrCopy + m.widthStep + nChannels)[1])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[1] + 2 * (dataPtrCopy + m.widthStep)[1] + (dataPtrCopy + m.widthStep + nChannels)[1]) -
                                ((dataPtrCopy - nChannels)[1] + 2 * (dataPtrCopy)[1] + (dataPtrCopy + nChannels)[1]));

                        tempArray[2] = Math.Abs(
                                ((dataPtrCopy - nChannels)[2] + 2 * (dataPtrCopy - nChannels)[2] + (dataPtrCopy + m.widthStep - nChannels)[2]) -
                                ((dataPtrCopy + nChannels)[2] + 2 * (dataPtrCopy + nChannels)[2] + (dataPtrCopy + m.widthStep + nChannels)[2])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[2] + 2 * (dataPtrCopy + m.widthStep)[2] + (dataPtrCopy + m.widthStep + nChannels)[2]) -
                                ((dataPtrCopy - nChannels)[2] + 2 * (dataPtrCopy)[2] + (dataPtrCopy + nChannels)[2]));


                        if (tempArray[0] > 255)
                        {
                            dataPtr[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr[0] = 0;
                        }
                        else
                        {
                            dataPtr[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr[1] = 0;
                        }
                        else
                        {
                            dataPtr[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr[2] = 0;
                        }
                        else
                        {
                            dataPtr[2] = (byte)tempArray[2];
                        }

                        dataPtr += nChannels;
                        dataPtrCopy += nChannels;
                    }

                    //Bottom Line

                    dataPtr = (byte*)m.imageData.ToPointer();
                    dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    dataPtr += nChannels + (height-1) * m.widthStep;
                    dataPtrCopy += nChannels + (height-1) * m.widthStep;

                    for (int x = 1; x < width - 1; x++)
                    {
                        tempArray[0] = Math.Abs(
                                ((dataPtrCopy - nChannels - m.widthStep)[0] + 2 * (dataPtrCopy - nChannels)[0] + (dataPtrCopy - nChannels)[0]) -
                                ((dataPtrCopy + nChannels - m.widthStep)[0] + 2 * (dataPtrCopy + nChannels)[0] + (dataPtrCopy + nChannels)[0])) +

                                Math.Abs(
                                ((dataPtrCopy - nChannels)[0] + 2 * (dataPtrCopy)[0] + (dataPtrCopy + nChannels)[0]) -
                                ((dataPtrCopy - nChannels - m.widthStep)[0] + 2 * (dataPtrCopy - m.widthStep)[0] + (dataPtrCopy + nChannels - m.widthStep)[0]));

                        tempArray[1] = Math.Abs(
                                ((dataPtrCopy - nChannels - m.widthStep)[1] + 2 * (dataPtrCopy - nChannels)[1] + (dataPtrCopy - nChannels)[1]) -
                                ((dataPtrCopy + nChannels - m.widthStep)[1] + 2 * (dataPtrCopy + nChannels)[1] + (dataPtrCopy + nChannels)[1])) +

                                Math.Abs(
                                ((dataPtrCopy - nChannels)[1] + 2 * (dataPtrCopy)[1] + (dataPtrCopy + nChannels)[1]) -
                                ((dataPtrCopy - nChannels - m.widthStep)[1] + 2 * (dataPtrCopy - m.widthStep)[1] + (dataPtrCopy + nChannels - m.widthStep)[1]));

                        tempArray[2] = Math.Abs(
                                ((dataPtrCopy - nChannels - m.widthStep)[2] + 2 * (dataPtrCopy - nChannels)[2] + (dataPtrCopy - nChannels)[2]) -
                                ((dataPtrCopy + nChannels - m.widthStep)[2] + 2 * (dataPtrCopy + nChannels)[2] + (dataPtrCopy + nChannels)[2])) +

                                Math.Abs(
                                ((dataPtrCopy - nChannels)[2] + 2 * (dataPtrCopy)[2] + (dataPtrCopy + nChannels)[2]) -
                                ((dataPtrCopy - nChannels - m.widthStep)[2] + 2 * (dataPtrCopy - m.widthStep)[2] + (dataPtrCopy + nChannels - m.widthStep)[2]));


                        if (tempArray[0] > 255)
                        {
                            dataPtr[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr[0] = 0;
                        }
                        else
                        {
                            dataPtr[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr[1] = 0;
                        }
                        else
                        {
                            dataPtr[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr[2] = 0;
                        }
                        else
                        {
                            dataPtr[2] = (byte)tempArray[2];
                        }

                        dataPtr += nChannels;
                        dataPtrCopy += nChannels;
                    }

                    //Left Column
                    dataPtr = (byte*)m.imageData.ToPointer();
                    dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    dataPtr += m.widthStep;
                    dataPtrCopy += m.widthStep;

                    for (int y = 1; y < height - 1; y++)
                    {
                        tempArray[0] = Math.Abs(
                                ((dataPtrCopy - m.widthStep)[0] + 2 * (dataPtrCopy)[0] + (dataPtrCopy + m.widthStep)[0]) -
                                ((dataPtrCopy + nChannels - m.widthStep)[0] + 2 * (dataPtrCopy + nChannels)[0] + (dataPtrCopy + nChannels + m.widthStep)[0])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep)[0] + 2 * (dataPtrCopy + m.widthStep)[0] + (dataPtrCopy + nChannels + m.widthStep)[0]) -
                                ((dataPtrCopy - m.widthStep)[0] + 2 * (dataPtrCopy - m.widthStep)[0] + (dataPtrCopy + nChannels - m.widthStep)[0]));

                        tempArray[1] = Math.Abs(
                                ((dataPtrCopy - m.widthStep)[1] + 2 * (dataPtrCopy)[1] + (dataPtrCopy + m.widthStep)[1]) -
                                ((dataPtrCopy + nChannels - m.widthStep)[1] + 2 * (dataPtrCopy + nChannels)[1] + (dataPtrCopy + nChannels + m.widthStep)[1])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep)[1] + 2 * (dataPtrCopy + m.widthStep)[1] + (dataPtrCopy + nChannels + m.widthStep)[1]) -
                                ((dataPtrCopy - m.widthStep)[1] + 2 * (dataPtrCopy - m.widthStep)[1] + (dataPtrCopy + nChannels - m.widthStep)[1]));

                        tempArray[2] = Math.Abs(
                                ((dataPtrCopy - m.widthStep)[2] + 2 * (dataPtrCopy)[2] + (dataPtrCopy + m.widthStep)[2]) -
                                ((dataPtrCopy + nChannels - m.widthStep)[2] + 2 * (dataPtrCopy + nChannels)[2] + (dataPtrCopy + nChannels + m.widthStep)[2])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep)[2] + 2 * (dataPtrCopy + m.widthStep)[2] + (dataPtrCopy + nChannels + m.widthStep)[2]) -
                                ((dataPtrCopy - m.widthStep)[2] + 2 * (dataPtrCopy - m.widthStep)[2] + (dataPtrCopy + nChannels - m.widthStep)[2]));


                        if (tempArray[0] > 255)
                        {
                            dataPtr[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr[0] = 0;
                        }
                        else
                        {
                            dataPtr[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr[1] = 0;
                        }
                        else
                        {
                            dataPtr[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr[2] = 0;
                        }
                        else
                        {
                            dataPtr[2] = (byte)tempArray[2];
                        }

                        dataPtr += m.widthStep;
                        dataPtrCopy += m.widthStep;
                    }

                    //Right Column
                    dataPtr = (byte*)m.imageData.ToPointer();
                    dataPtrCopy = (byte*)mCopy.imageData.ToPointer();

                    dataPtr += m.widthStep + (width-1) * nChannels;
                    dataPtrCopy += m.widthStep + (width-1) * nChannels;

                    for (int y = 1; y < height - 1; y++)
                    {
                        tempArray[0] = Math.Abs(
                                ((dataPtrCopy - m.widthStep - nChannels)[0] + 2 * (dataPtrCopy - nChannels)[0] + (dataPtrCopy + m.widthStep - nChannels)[0]) -
                                ((dataPtrCopy - m.widthStep)[0] + 2 * (dataPtrCopy)[0] + (dataPtrCopy+ m.widthStep)[0])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[0] + 2 * (dataPtrCopy + m.widthStep)[0] + (dataPtrCopy + m.widthStep)[0]) -
                                ((dataPtrCopy - m.widthStep - nChannels)[0] + 2 * (dataPtrCopy - m.widthStep)[0] + (dataPtrCopy - m.widthStep)[0]));

                        tempArray[1] = Math.Abs(
                                ((dataPtrCopy - m.widthStep - nChannels)[1] + 2 * (dataPtrCopy - nChannels)[1] + (dataPtrCopy + m.widthStep - nChannels)[1]) -
                                ((dataPtrCopy - m.widthStep)[1] + 2 * (dataPtrCopy)[1] + (dataPtrCopy+ m.widthStep)[1])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[1] + 2 * (dataPtrCopy + m.widthStep)[1] + (dataPtrCopy + m.widthStep)[1]) -
                                ((dataPtrCopy - m.widthStep - nChannels)[1] + 2 * (dataPtrCopy - m.widthStep)[1] + (dataPtrCopy - m.widthStep)[1]));

                        tempArray[2] = Math.Abs(
                                ((dataPtrCopy - m.widthStep - nChannels)[2] + 2 * (dataPtrCopy - nChannels)[2] + (dataPtrCopy + m.widthStep - nChannels)[2]) -
                                ((dataPtrCopy - m.widthStep)[2] + 2 * (dataPtrCopy)[2] + (dataPtrCopy+ m.widthStep)[2])) +

                                Math.Abs(
                                ((dataPtrCopy + m.widthStep - nChannels)[2] + 2 * (dataPtrCopy + m.widthStep)[2] + (dataPtrCopy + m.widthStep)[2]) -
                                ((dataPtrCopy - m.widthStep - nChannels)[2] + 2 * (dataPtrCopy - m.widthStep)[2] + (dataPtrCopy - m.widthStep)[2]));


                        if (tempArray[0] > 255)
                        {
                            dataPtr[0] = 255;
                        }
                        else if (tempArray[0] < 0)
                        {
                            dataPtr[0] = 0;
                        }
                        else
                        {
                            dataPtr[0] = (byte)tempArray[0];
                        }

                        if (tempArray[1] > 255)
                        {
                            dataPtr[1] = 255;
                        }
                        else if (tempArray[1] < 0)
                        {
                            dataPtr[1] = 0;
                        }
                        else
                        {
                            dataPtr[1] = (byte)tempArray[1];
                        }

                        if (tempArray[2] > 255)
                        {
                            dataPtr[2] = 255;
                        }
                        else if (tempArray[2] < 0)
                        {
                            dataPtr[2] = 0;
                        }
                        else
                        {
                            dataPtr[2] = (byte)tempArray[2];
                        }

                        dataPtr += m.widthStep;
                        dataPtrCopy += m.widthStep;
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
            unsafe
            {
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
                int bestThreshold = 0;

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

                        for (int i = t + 1; i <= 255; i++)
                        {
                            q2 += histogram[i];
                            u2 += i * histogram[i];
                        }
                        u2 /= q2;

                        double result = q1 * q2 * (u1 - u2) * (u1 - u2);
                        if (result >= bestVar)
                        {
                            bestThreshold = t;
                            bestVar = result;
                        }
                    }
                    ConvertToBW(img, bestThreshold);
                }
            }
        }
    }
}
