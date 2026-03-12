<template lang="html">
  <v-app style="background: #DCDCDC">
    <v-snackbar
      top
      :value="stateShowError"
      @input="setStateError({showError: $event})"
    >
      {{ stateErrorMessage }}
      <v-btn color="error" icon @click="setStateError({showError: false})">
        <v-icon>close</v-icon>
      </v-btn>
    </v-snackbar>
    <toolbar-component />
    <v-content>
      <v-container fluid>
        <nuxt />
      </v-container>
    </v-content>
    <v-footer style="background: #A9A9A9" app>
      <v-layout justify-center>
        &copy;&nbsp;2018-2021
      </v-layout>
    </v-footer>
  </v-app>
</template>

<script lang="ts">
import {Component, Mutation, State, Vue} from "nuxt-property-decorator";

import ToolbarComponent from "~/components/toolbar.component.vue";

@Component({components: {ToolbarComponent}, middleware: "auth"})
export default class DefaultLayout extends Vue {
  @State("errorMessage")
  public stateErrorMessage!: string;

  @State("showError")
  public stateShowError!: string;

  @Mutation("SET_ERROR")
  public setStateError!: (payload: {
    errorMessage?: string;
    showError?: boolean;
  }) => void;
}
</script>

<style lang="css">
a {
  text-decoration: none;
}

</style>
