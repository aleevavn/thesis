#pragma omp parallel for private(i, j, k)
for (int i = 0; i < Size; i++)
    for (int j = 0; j < Size; j++)
        for (int k = 0; k < Size; k++)
            Ñ[i][j][k] = A[i][k] * B[k][j];

int iterations = Size;
for (int g = 1; g < Size; g *= 2) 
{
    iterations /= 2;
#pragma omp parallel for private (i, j, k, cell)
    for (int i = 0; i < Size; i++)
        for (int j = 0; j < Size; j++)
            for (int k = 0; k < iterations; k++) 
            {
                int cell = g * k;
                C[i][j][cell] += C[i][j][cell + g];
                C[i][j][cell + g] = 0;
            }
}
