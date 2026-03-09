import matplotlib.pyplot as plt
from Approximation import FunctorListMethods
import decimal
import numpy
from io import BytesIO


class Schedule:
    def __init__(self, koefficients, functorList, parameters, results, legend_type, xMin=None, xMax=None, yMin=None, yMax=None):
        self.SetParameters(koefficients, functorList, parameters, results, legend_type, xMin, xMax, yMin, yMax);

        plt.figure(figsize=(9.6, 7.2));
        if(not self.CanShow()):
            self.graphPlot = None;
            return;

        if (self.dimentions_ == 1):
            self.graphPlot = plt.axes([0.1, 0.1, 0.85, 0.85])
        elif(self.dimentions_ == 2):
            self.graphPlot = plt.axes([0.05, 0.05, 0.9, 0.9], projection='3d')

        self.Update()

    def SetKoefficientsAndFunctorList(self, koefficients, functorList):
        self.koefficients_ = koefficients;
        self.functorList_ = functorList;
        self.dimentions_ = len(functorList.GetConformity());


    def SetParameters(self, koefficients, functions, parameters, results,legend_type, minX, maxX, minY, maxY):
        self.parameters_ = parameters;
        self.results_ = results;
        self.legend_type = legend_type;
        self.SetKoefficientsAndFunctorList(koefficients, functions);
        self.SetRange(minX, maxX, minY, maxY);

    def SetRange(self, xMin, xMax, yMin, yMax):
        minPoint = min(self.parameters_)[0]
        maxPoint = max(self.parameters_)[0]
        if(minPoint == 0 and maxPoint == 0):
            minPoint = 0;
            maxPoint = 1;
        if(xMin == None): xMin = minPoint;
        if(xMax == None): xMax = maxPoint;
        if(yMin == None): yMin = minPoint;
        if(yMax == None): yMax = maxPoint;

        self.xMin_ = xMin;
        self.xMax_ = xMax;
        self.yMin_ = yMin;
        self.yMax_ = yMax;

    def Update(self):
        self.graphPlot.clear();
        if(self.dimentions_ == 0):
            x, y = self.Get2D_Data();
            self.graphPlot.plot(x, y);
            xPoints = [];
            conformity = 0;
            for i in range(len(self.parameters_)):
                xPoints.append(self.parameters_[i][conformity]);
            yPoints = [];
            for i in range(len(self.results_)):
                yPoints.append(self.results_[i]);
            plt.scatter(xPoints, yPoints, c='r');

        elif(self.dimentions_ == 1):
            x, y = self.Get2D_Data();
            self.graphPlot.plot(x, y);
            xPoints = [];
            conformity = self.functorList_.GetConformity()[0];
            for i in range(len(self.parameters_)):
                xPoints.append(self.parameters_[i][conformity]);
            yPoints = [];
            for i in range(len(self.results_)):
                yPoints.append(self.results_[i]);
            plt.scatter(xPoints, yPoints, c='r');
            if (self.functorList_.ToString() == "x2"):
                self.graphPlot.set_xlabel(r'$X_{2}$');
            else:
                self.graphPlot.set_xlabel(r'$X_{1}$');
            self.graphPlot.set_ylabel(self.legend_type);

        elif(self.dimentions_ == 2):
            x, y, z = self.Get3D_Data();
            surf = self.graphPlot.plot_surface(x, y, z);

            # #Следующие 2 строки нужны чтобы избежать ошибки: 'Poly3DCollection' object has no attribute... Это ошибка в реализации библиотеки
            # surf._facecolors2d=surf._facecolors3d;
            # surf._edgecolors2d=surf._edgecolors3d;
            self.graphPlot.set_xlabel(r'$X_{1}$');
            self.graphPlot.set_ylabel(r'$X_{2}$');
            self.graphPlot.set_zlabel(self.legend_type);

        self.graphPlot.grid(True);
        plt.draw();


    def drange_(x, y, jump):
        while x < y:
            yield float(x)
            x += decimal.Decimal(jump)

    def Get2D_Data(self):
        x = [];
        y = [];
        step = (int(self.xMax_) + 1 - int(self.xMin_)) / 100;
        for i in Schedule.drange_(int(self.xMin_), int(self.xMax_) + step, step):
            x.append(i);
            sum = 0;

            conformity = self.functorList_.GetConformity();
            for j in range(len(self.functorList_)):

                if(len(conformity) == 0):
                    sum += self.koefficients_[j] * self.functorList_[j]([]);
                else:
                    data = dict();
                    data[conformity[0]] = i;
                    sum += self.koefficients_[j] * self.functorList_[j](data);
            y.append(sum);
        return x, y;

    def Get3D_Data(self):
        stepX = (int(self.xMax_) + 1 - int(self.xMin_)) / 100;
        x = numpy.arange (int(self.xMin_), int(self.xMax_) + 1, stepX);
        stepY = (int(self.yMax_) + 1 - int(self.yMin_)) / 100;
        y = numpy.arange (int(self.yMin_), int(self.yMax_) + 1, stepY);
        xgrid, ygrid = numpy.meshgrid(x, y);

        matrix = [];

        for i in x:
            row = [];
            for j in y:
                data = dict();
                conformity = self.functorList_.GetConformity();
                data[conformity[0]] = i;
                data[conformity[1]] = j;

                row.append(FunctorListMethods.CalculateDependence(self.koefficients_, self.functorList_, data));

            matrix.append(row);
        zgrid = numpy.array(matrix);
        return xgrid, ygrid, zgrid;

    def CanShow(self):
        return self.dimentions_ == 1 or self.dimentions_ == 2;

    def Show(self):
        plt.show();

    def SaveToDisk(self, fileName):
        plt.savefig(fileName);

    def SaveToBuffer(self, buffer : BytesIO, format : str):
        plt.savefig(buffer, format=format);
