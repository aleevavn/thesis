#include "stdafx.h"
#include <iostream>
#include <fstream>
#include <ctime>


using namespace std;

int main(int argc, char* argv[])
{
	srand(time(0));

	ifstream file1("array.txt");
	int N, perem = 0, gperem = 0, sperem = 0, sgperem = 0;
	const double PI = 3.14159265;

	file1 >> N;

	int L = N, n1 = 1, tao = 1;


	double *F = new double[N];
	double *Ans = new double[N];

	for (int i = 0; i < N; i++)
	{
		file1 >> F[i];
	}
#pragma omp parallel for
	for (int j = 1; j < N-2; j++)
	{

		for (int k = 0; k < N; k++)
		{
			for (int i = 0; i < N; i++)
			{
				perem = F[i] * sqrt(2 / L)*sin(PI * k * i / L);
				gperem = gperem + perem;
			}

			sperem = gperem * sqrt(2 / L) * sin(PI * k * n1 * tao / L) / (4 / (tao * tao) * sin(PI * k * tao / 2 / L) * (PI * k * tao / 2 / L));
			sgperem = sgperem + sperem;
		
		}
	
		Ans[j] = sgperem;
		n1++;
	}

	Ans[0] = 0;
	Ans[N-1] = 0;
	ofstream file2("output.txt");


	for (int i = 0; i < N; i++) 
       {
		file2 << Ans[i] << " ";
	}

	file2 << "runtime = " << clock() / 1000.0 << endl; // גנולÿ נאבמעû ןנמדנאללû                  
	system("pause");
	return 0;
}


