<template lang="html">
  <div>
    <v-breadcrumbs class="remove-top-padding" :items="breadcrumbItems">
      <template v-slot:divider>
        <v-icon>chevron_right</v-icon>
      </template>
    </v-breadcrumbs>
    <v-form ref="argumentsForm">
      <v-text-field
        v-for="argument in argumentsData"
        :key="argument.index"
        v-model.number="argument.value"
        :label="`x${argument.index}`"
        :rules="rules.argument"
        type="number"
      />
    </v-form>

    <div class="d-inline-block" v-if="approximationTicksData.img !== ''">
      <v-img
        :src="approximationTicksData.img"
        width="480"
        height="360"
        contain
      ></v-img>
    </div>
    <div class="d-inline-block" v-if="approximationTicksData.img !== ''">
      <v-img
        :src="approximationProcessorsData.img"
        width="480"
        height="360"
        contain
      ></v-img>
    </div>

    <v-expansion-panel class="d-inline" style="width: 50%">
      <v-expansion-panel-content>
        <template v-slot:header>
          <div>
            {{
              $t("$.approximation.ticks", {
                value: Math.ceil(calculateJson(approximationTicksData.json)),
              })
            }}
          </div>
        </template>
        <div class="flex items-center pl-4">
          <katex-element :expression="approximationTicksData.latex" />
        </div>
      </v-expansion-panel-content>
    </v-expansion-panel>

    <v-expansion-panel class="d-inline" style="width: 50%">
      <v-expansion-panel-content>
        <template v-slot:header>
          <div>
            {{
              $t("$.approximation.processors", {
                value: Math.round(calculateJson(approximationProcessorsData.json)),
              })
            }}
          </div>
        </template>
        <div class="flex items-center pl-4">
          <katex-element :expression="approximationProcessorsData.latex" />
        </div>
      </v-expansion-panel-content>
    </v-expansion-panel>
  </div>
</template>

<script lang="ts">
import axios from "axios";
import {Component, Vue} from "nuxt-property-decorator";

@Component({
  async asyncData(context) {
    const [resAlgorithm, resMatrix] = await Promise.all([
      axios.get(`/algorithms/${context.params.algorithmId}`, {
        baseURL: process.env.API_ENDPOINT,
      }),
      axios.get(
        `/algorithms/${context.params.algorithmId}/determinants/matrix`,
        {
          baseURL: process.env.API_ENDPOINT,
        },
      ),
    ]);

    if (
      !resMatrix.data.data.X.length ||
      [resMatrix.data.data.y.processors, resMatrix.data.data.y.ticks].some(
        (y: number[]) => !y.length || y.some((item) => item === null),
      )
    ) {
      return {
        algorithmData: resAlgorithm.data.data,
        argumentsData: [],
        approximationProcessorsData: {
          json: [],
          latex: "\\varnothing",
          img: "",
        },
        approximationTicksData: {
          json: [],
          latex: "\\varnothing",
          img: "",
        },
      };
    }

    const [
      resApproximationProcessors,
      resApproximationTicks,
    ] = await Promise.all([
      axios.get(`/malgorithms/${context.params.algorithmId}?type=processors`, {
        baseURL: process.env.API_ENDPOINT,
      }),
      axios.get(`/malgorithms/${context.params.algorithmId}?type=ticks`, {
        baseURL: process.env.API_ENDPOINT,
      }),
    ]);

    const argumentsData: Array<{index: number; value: number}> = [];
    [
      resApproximationProcessors.data.data.json,
      resApproximationTicks.data.data.json,
    ].map((json) =>
      json.map((d: any) =>
        d.variables.map((v: any) => {
          if (argumentsData.findIndex((a) => a.index === v.index) < 0) {
            argumentsData.push({index: v.index, value: 0});
          }
        }),
      ),
    );

    argumentsData.sort((a, b) =>
      a.index < b.index ? -1 : a.index === b.index ? 0 : 1,
    );

    return {
      algorithmData: resAlgorithm.data.data,
      argumentsData,
      approximationProcessorsData: {
        json: resApproximationProcessors.data.data.json,
        latex: resApproximationProcessors.data.data.latex,
        img: resApproximationProcessors.data.data.img,
      },
      approximationTicksData: {
        json: resApproximationTicks.data.data.json,
        latex: resApproximationTicks.data.data.latex,
        img: resApproximationTicks.data.data.img,
      },
    };
  },
})
export default class ApproximationPage extends Vue {
  public algorithmData!: any;
  public argumentsData!: Array<{index: number; value: number}>;

  public get rules() {
    return {
      argument: [
        (v: string) => v !== "" || this.$t("$.rules.argument.required"),
      ],
    };
  }

  public get breadcrumbItems() {
    return [
      {
        exact: true,
        nuxt: true,
        text: this.$t("$.approximation.breadcrumbs.prev"),
        to: "/",
      },
      {
        exact: true,
        nuxt: true,
        text: this.$t("$.approximation.breadcrumbs.cur", {
          n: this.algorithmData.id,
          name: this.algorithmData.name,
        }),
        to: `/algorithms/${this.algorithmData.id}/determinants/approximation/${
          (this as any).$route.params.degree
        }`,
      },
    ];
  }

  public calculateJson(json: any) {
    if (
      !json.length ||
      (this.$refs.argumentsForm &&
        !(this.$refs.argumentsForm as any).validate())
    ) {
      return "?";
    }
    return json.reduce(
      (r: number, d: any) =>
        r +
        d.variables.reduce(
          (x: number, v: any) =>
            v.log2
              ? x *
                Math.log2(
                  (this.argumentsData.find((a) => a.index === v.index) as any)
                    .value,
                ) **
                  v.pow
              : x *
                (this.argumentsData.find((a) => a.index === v.index) as any)
                  .value **
                  v.pow,
          d.coef,
        ),
      0,
    );
  }
}
</script>

<style lang="css" scoped>
.remove-top-padding {
  padding-top: 0 !important;
}
</style>
