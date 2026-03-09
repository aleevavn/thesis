import axios from "axios";
import {celebrate} from "celebrate";
import Router from "express-promise-router";
import * as assert from "http-assert";
import * as mongoose from "mongoose";
import {
  Algorithm,
  ApiCodes,
  authViaAuthToken,
  bodyParser,
  Determinant,
  Joi,
} from "src";
import malgorithmSchemas from "../../../../src/mongoDB/models/malgorithmSchemas";

const router = Router({
  mergeParams: true,
});
export default router;

/**
 * @api {patch} /malgorithms/:malgorithmId Изменить алгоритм
 * @apiVersion 2.0.0
 * @apiName ChangeAlgorithm
 * @apiGroup MAlgorithms
 * @apiPermission Пользователь
 *
 * @apiParam {string="1..9223372036854775807"} :malgorithmId ID алгоритма
 * @apiParam {string} [name] Имя алгоритма
 * @apiParam {string} [description] Описание алгоритма
 *
 * @apiUse v200CommonErrors
 * @apiUse v200AuthViaAuthToken
 */

router.use(authViaAuthToken, bodyParser(), async (req, res) => {
  const determinants = await Determinant.findAll({
    attributes: ["dimensions", "iterations", "processors", "ticks"],
    order: [["id", "ASC"]],
    where: {
      algorithmId: req.params.malgorithmId,
    },
  });

  const data = {
    X: determinants.map((determinant) => [
      ...determinant.dimensions, // x1..xn-1
      determinant.iterations, // xn
    ]),
    y: {
      processors: determinants.map((determinant) => determinant.processors),
      ticks: determinants.map((determinant) => determinant.ticks),
    },
  };

  const [resApproximationProcessors, resApproximationTicks] = await Promise.all(
    [
      axios.post(
        `/api/v2/approximation`,
        {
          X: data.X,
          y: data.y.processors,
          legend_type: "Width",
        },
        {
          baseURL: process.env.API_COPROXIMATION_SERVER,
        },
      ),
      axios.post(
        `/api/v2/approximation`,
        {
          X: data.X,
          y: data.y.ticks,
          legend_type: "Height",
        },
        {
          baseURL: process.env.API_COPROXIMATION_SERVER,
        },
      ),
    ],
  );

  const malgorithm = await malgorithmSchemas.findOneAndUpdate(
    {
      index: Number(req.params.malgorithmId),
    },
    {
      data_height: resApproximationProcessors.data.data,

      data_width: resApproximationTicks.data.data,
    },
  );
  res.status(200).send(malgorithm);
});
