using namespace std;
class Equations
{
	public:
		Equations(int size);// конструктор для создания задачи из конкретных данных
		Equations();//конструктор по умолчанию для создания задачи фиксированной размерности заполненной случайно
		void JacobiOpenMP();
		void Print();
	private:
		vector<vector<double>> A;
		vector<double> b, preX, x;
		int size;
		double time;
		void keyboardInit();
		void randomInit();
		double Norm(double* X1, double* X2)
};

