import {celebrate} from "celebrate";
import Router from "express-promise-router";

import {Algorithm, authViaAuthToken, bodyParser, Joi} from "src";
import malgorithmSchemas from "../../../src/mongoDB/models/malgorithmSchemas";

const router = Router();
export default router;

/**
 * @api {post} /algorithms Создать новый алгоритм
 * @apiVersion 2.0.0
 * @apiName CreateAlgorithm
 * @apiGroup Algorithms
 * @apiPermission Пользователь
 *
 * @apiParam {string} name Имя алгоритма
 * @apiParam {string} description Описание алгоритма
 *
 * @apiSuccess (Created 201) {string="1..9223372036854775807"} id ID алгоритма
 *
 * @apiUse v200CommonErrors
 * @apiUse v200AuthViaAuthToken
 */
router.use(
  authViaAuthToken,
  bodyParser(),
  celebrate({
    body: Joi.object({
      description: Joi.string().required(),
      name: Joi.string().required(),
    }),
  }),
  async (req, res) => {
    const algorithm = await Algorithm.create({
      description: req.body.description,
      name: req.body.name,
    });

    const malgorithm = await malgorithmSchemas.create({
      data_height: {
        coef: [],
        img: "",
        json: [],
        latex: "",
        names: [],
      },
      data_width: [],
      description: req.body.description,
      determinantsCount: 0,
      index: algorithm.id,
      name: req.body.name,
    });

    res.status(201).json({
      id: algorithm.id,
    });
  },
);
