double *tmpC = new double[Size * bufSize]
MPI_Scatter(A, bufSize, MPI_DOUBLE, bufA, bufSize, MPI_DOUBLE, 0, 
                                                          MPI_COMM_WORLD);
MPI_Scatter(B, bufSize, MPI_DOUBLE, bufB, bufSize, MPI_DOUBLE, 0, 
                                                          MPI_COMM_WORLD);

#pragma omp parallel for private(j,k)
for (int i = 0; i < part; i++)
    for (int j = 0; j < part; j++)
        for (int k = 0; k < Size; k++)
           tmpC[(i * Size + j) * Size + k] = bufA[i * Size + k] * 
                                                       bufB[k * Size + j];
int iterations = Size;

for (int pwr = 1; pwr < Size; pwr *= 2) 
{
    iterations /= 2;
#pragma omp parallel for private(j,k,cell)
    for (int i = 0; i < part; i++)
        for (int j = 0; j < part; j++)
            for (int k = 0; k < iterations; k++) {
                int cell = pwr * k;
                tmpC[(i * Size + j) * Size + cell] += 
                                 bufC[(i * Size + j) * Size + cell + pwr];
                if (iterations < 2)
                    bufC[i * Size + j] = tmpC[(i * Size + j) * Size];
                }
MPI_Gather(bufC, bufSize, MPI_DOUBLE, C, bufSize, MPI_DOUBLE, 0, 
                                                         MPI_COMM_WORLD);
