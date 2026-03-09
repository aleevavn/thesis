"""
Flask application
"""

import os

from dotenv import load_dotenv
from flask import Flask, request, Response
from flask_cors import CORS

import json

from Approximation.GuessApproximation import GuessApproximation
from Approximation.OutputData import OutputData
from Schedule.Schedule import Schedule

load_dotenv()
APP = Flask(__name__, static_folder="apidoc", static_url_path="")
CORS(APP)

@APP.route("/", methods=["GET"])
def root():
    """
    Send index.html
    """
    return APP.send_static_file("index.html")


@APP.route("/api/v2/approximation", methods=["POST"])
def hello():
    """
       @api {post} /approximation Получить аппроксимацию
       @apiVersion 2.0.0
       @apiName GetApproximation
       @apiGroup Approximation
       @apiPermission Гость

       @apiParam {number[][]} X Аргументы функции
       @apiParam {number[]} y Результаты функции
       @apiParam {number} degree Степень многочлена
       @apiParam {string} legend_type Подпись вертикальной оси

       @apiSuccess {object} data Данные
       @apiSuccess {number[]} data.coef Коэффициенты
       @apiSuccess {string[]} data.names Имена
       @apiSuccess {object[]} data.json Многочлен в JSON формате
       @apiSuccess {number} data.json.coef Коэффициент
       @apiSuccess {object[]} data.json.variables Переменные
       @apiSuccess {number} data.json.variables.index Индекс переменной
       @apiSuccess {number} data.json.variables.pow Степень переменной
       @apiSuccess {string} data.latex Многочлен в Latex формате
       """
    # approximation = Approximation.fit(request.json["X"], request.json["y"], request.json["degree"])
    parameters= request.json["X"];
    processors = request.json["y"];
    legend_type = request.json["legend_type"];
    koefficients, functorsList, discripancy = GuessApproximation.Analyse(parameters, processors, fullSearch=False,
                                                                         bDebug=False);
    schedule = Schedule(koefficients, functorsList, parameters, processors,legend_type);
    outputData  = OutputData(koefficients, functorsList, schedule);



    return Response(json.dumps(outputData.data), 200, mimetype="application/json")


if __name__ == '__main__':
    APP.run(os.getenv("HOST", "0.0.0.0"), int(os.getenv("PORT", "8081")))
